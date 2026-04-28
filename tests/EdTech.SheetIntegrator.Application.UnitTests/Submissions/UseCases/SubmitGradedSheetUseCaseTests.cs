using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;
using EdTech.SheetIntegrator.Application.Submissions.UseCases;
using EdTech.SheetIntegrator.Application.Submissions.Validators;
using EdTech.SheetIntegrator.Application.UnitTests.TestData;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EdTech.SheetIntegrator.Application.UnitTests.Submissions.UseCases;

public class SubmitGradedSheetUseCaseTests
{
    private readonly IAssessmentRepository _assessments = Substitute.For<IAssessmentRepository>();
    private readonly ISubmissionRepository _submissions = Substitute.For<ISubmissionRepository>();
    private readonly ISheetParserFactory _parserFactory = Substitute.For<ISheetParserFactory>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly FakeClock _clock = new(AppFixtures.Now);

    private SubmitGradedSheetUseCase Build() => new(
        _assessments, _submissions, _parserFactory, _uow, _clock,
        new SubmitGradedSheetRequestValidator(),
        NullLogger<SubmitGradedSheetUseCase>.Instance);

    private static Assessment SingleQuestionAssessment(Guid id, string answer = "Paris", decimal points = 1m) =>
        new(id, "Quiz",
            [new Question("Q1", "Capital of France?", answer, points, MatchMode.Exact)],
            AppFixtures.Now);

    private static ISheetParser ParserReturning(IReadOnlyList<RawAnswer> answers)
    {
        var parser = Substitute.For<ISheetParser>();
        parser.ParseAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(answers);
        return parser;
    }

    [Fact]
    public async Task Returns_Validation_Error_When_StudentIdentifier_Empty()
    {
        var assessmentId = Guid.NewGuid();
        var sut = Build();

        var result = await sut.ExecuteAsync(
            new(assessmentId, "", "alice.xlsx", "x", new MemoryStream()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("validation.failed");
        await _assessments.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_NotFound_When_Assessment_Missing()
    {
        _assessments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Assessment?)null);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("assessment.not_found");
    }

    [Fact]
    public async Task Returns_UnsupportedFileType_When_Factory_Returns_Null()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid());
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns((ISheetParser?)null);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(assessment.Id, sourceFileName: "alice.txt"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("submission.unsupported_file_type");
    }

    [Fact]
    public async Task Returns_UnreadableSheet_When_Parser_Throws_SheetParsingException()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid());
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var parser = Substitute.For<ISheetParser>();
        parser.ParseAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<RawAnswer>>>(_ => throw new SheetParsingException("corrupt header"));
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns(parser);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(assessment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("submission.unreadable_sheet");
        result.Error.Message.Should().Contain("corrupt");
    }

    [Fact]
    public async Task Returns_InvalidSubmission_When_Parser_Returns_Empty_Answers()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid());
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var parser = ParserReturning([]);
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns(parser);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(assessment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("submission.invalid");
    }

    [Fact]
    public async Task Returns_MismatchedAnswerSheet_When_Answers_Reference_Unknown_Question()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid());
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var parser = ParserReturning([new RawAnswer("Q1", "Paris"), new RawAnswer("Q99", "ghost")]);
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns(parser);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(assessment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("submission.mismatched_answer_sheet");
    }

    [Fact]
    public async Task Happy_Path_Persists_Graded_Submission_And_Returns_Score()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid(), answer: "Paris", points: 1m);
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var parser = ParserReturning([new RawAnswer("Q1", "Paris")]);
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns(parser);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(assessment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsGraded.Should().BeTrue();
        result.Value.Earned.Should().Be(1m);
        result.Value.Total.Should().Be(1m);
        result.Value.Percentage.Should().Be(100m);
        result.Value.GradedAt.Should().Be(AppFixtures.Now);
        result.Value.Outcomes.Should().ContainSingle().Which.IsCorrect.Should().BeTrue();

        await _submissions.Received(1).AddAsync(
            Arg.Is<StudentSubmission>(s => s.IsGraded && s.Result!.Score.Earned == 1m),
            Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Happy_Path_Records_Source_File_Name_On_Submission()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid());
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var parser = ParserReturning([new RawAnswer("Q1", "Paris")]);
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns(parser);
        var sut = Build();

        var result = await sut.ExecuteAsync(
            AppFixtures.SubmitRequest(assessment.Id, sourceFileName: "uploads/winter-2026-final.xlsx"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SourceFileName.Should().Be("uploads/winter-2026-final.xlsx");
    }

    [Fact]
    public async Task Does_Not_Persist_When_Grading_Fails()
    {
        var assessment = SingleQuestionAssessment(Guid.NewGuid());
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var parser = ParserReturning([new RawAnswer("Q99", "ghost")]);
        _parserFactory.Resolve(Arg.Any<string>(), Arg.Any<string?>()).Returns(parser);
        var sut = Build();

        var result = await sut.ExecuteAsync(AppFixtures.SubmitRequest(assessment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _submissions.DidNotReceive().AddAsync(Arg.Any<StudentSubmission>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
