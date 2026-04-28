using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Application.Assessments.UseCases;
using EdTech.SheetIntegrator.Application.Assessments.Validators;
using EdTech.SheetIntegrator.Application.UnitTests.TestData;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Grading;
using Microsoft.Extensions.Logging.Abstractions;

namespace EdTech.SheetIntegrator.Application.UnitTests.Assessments.UseCases;

public class CreateAssessmentUseCaseTests
{
    private readonly IAssessmentRepository _repo = Substitute.For<IAssessmentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly FakeClock _clock = new(AppFixtures.Now);

    private CreateAssessmentUseCase Build() => new(
        _repo, _uow, _clock,
        new CreateAssessmentRequestValidator(),
        NullLogger<CreateAssessmentUseCase>.Instance);

    [Fact]
    public async Task Returns_Validation_Error_When_Title_Empty()
    {
        var sut = Build();

        var result = await sut.ExecuteAsync(
            AppFixtures.CreateAssessmentRequest(title: ""),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("validation.failed");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Assessment>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Persists_New_Assessment_And_Returns_Response_On_Happy_Path()
    {
        var sut = Build();
        var request = AppFixtures.CreateAssessmentRequest(
            title: "Quiz",
            questions: [
                AppFixtures.ExactQuestionInput(id: "Q1", points: 1m),
                AppFixtures.NumericQuestionInput(id: "Q2", points: 5m, tolerance: 0.01m),
            ]);

        var result = await sut.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Quiz");
        result.Value.MaxScore.Should().Be(6m);
        result.Value.Questions.Should().HaveCount(2);
        result.Value.CreatedAt.Should().Be(AppFixtures.Now);
        result.Value.Id.Should().NotBe(Guid.Empty);

        await _repo.Received(1).AddAsync(
            Arg.Is<Assessment>(a => a.Title == "Quiz" && a.Questions.Count == 2),
            Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_InvalidConfiguration_When_Domain_Rejects_Construction()
    {
        // Two questions with the same id pass FluentValidation (shape is fine), but the Domain
        // throws InvalidQuestionConfigurationException for duplicate ids. This proves the
        // catch-and-translate behavior on the use case.
        var sut = Build();
        var request = AppFixtures.CreateAssessmentRequest(
            questions: [
                AppFixtures.ExactQuestionInput(id: "Q1"),
                AppFixtures.ExactQuestionInput(id: "Q1"),
            ]);

        var result = await sut.ExecuteAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("assessment.invalid_configuration");
        result.Error.Message.Should().Contain("Duplicate");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Assessment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Generates_A_New_Guid_For_Each_Created_Assessment()
    {
        var sut = Build();
        var request = AppFixtures.CreateAssessmentRequest();

        var first = await sut.ExecuteAsync(request, CancellationToken.None);
        var second = await sut.ExecuteAsync(request, CancellationToken.None);

        first.Value.Id.Should().NotBe(second.Value.Id);
    }

    [Fact]
    public async Task Maps_MatchMode_And_Tolerance_Through_To_Response()
    {
        var sut = Build();
        var request = AppFixtures.CreateAssessmentRequest(
            questions: [AppFixtures.NumericQuestionInput(id: "Q1", tolerance: 0.5m)]);

        var result = await sut.ExecuteAsync(request, CancellationToken.None);

        var q = result.Value.Questions.Single();
        q.MatchMode.Should().Be(MatchMode.Numeric);
        q.NumericTolerance.Should().Be(0.5m);
    }
}
