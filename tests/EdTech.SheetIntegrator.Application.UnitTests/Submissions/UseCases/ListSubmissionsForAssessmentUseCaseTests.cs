using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using EdTech.SheetIntegrator.Application.Submissions.UseCases;
using EdTech.SheetIntegrator.Application.Submissions.Validators;
using EdTech.SheetIntegrator.Application.UnitTests.TestData;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Application.UnitTests.Submissions.UseCases;

public class ListSubmissionsForAssessmentUseCaseTests
{
    private readonly ISubmissionRepository _submissions = Substitute.For<ISubmissionRepository>();
    private readonly IAssessmentRepository _assessments = Substitute.For<IAssessmentRepository>();

    private ListSubmissionsForAssessmentUseCase Build() => new(
        _submissions, _assessments, new ListSubmissionsRequestValidator());

    [Fact]
    public async Task Returns_Validation_Error_When_PageSize_Zero()
    {
        var sut = Build();

        var result = await sut.ExecuteAsync(new ListSubmissionsRequest(Guid.NewGuid(), PageSize: 0), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("validation.failed");
        await _assessments.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_NotFound_When_Assessment_Missing()
    {
        _assessments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Assessment?)null);
        var sut = Build();

        var result = await sut.ExecuteAsync(new ListSubmissionsRequest(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("assessment.not_found");
    }

    [Fact]
    public async Task Returns_Paged_Submissions_On_Happy_Path()
    {
        var assessment = AppFixtures.Assessment();
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);

        var subs = new List<StudentSubmission>
        {
            AppFixtures.Submission(assessmentId: assessment.Id, identifier: "a@x.com"),
            AppFixtures.Submission(assessmentId: assessment.Id, identifier: "b@x.com"),
        };
        _submissions.ListByAssessmentAsync(assessment.Id, 0, 20, Arg.Any<CancellationToken>())
            .Returns(subs);
        _submissions.CountByAssessmentAsync(assessment.Id, Arg.Any<CancellationToken>())
            .Returns(57);

        var sut = Build();

        var result = await sut.ExecuteAsync(
            new ListSubmissionsRequest(assessment.Id, Page: 1, PageSize: 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalCount.Should().Be(57);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Computes_Skip_From_Page_And_PageSize()
    {
        var assessment = AppFixtures.Assessment();
        _assessments.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        _submissions.ListByAssessmentAsync(assessment.Id, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _submissions.CountByAssessmentAsync(assessment.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        var sut = Build();

        await sut.ExecuteAsync(new ListSubmissionsRequest(assessment.Id, Page: 3, PageSize: 25), CancellationToken.None);

        await _submissions.Received(1).ListByAssessmentAsync(
            assessment.Id, skip: 50, take: 25, Arg.Any<CancellationToken>());
    }
}
