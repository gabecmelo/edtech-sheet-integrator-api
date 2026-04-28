using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using EdTech.SheetIntegrator.Application.Submissions.UseCases;
using EdTech.SheetIntegrator.Application.UnitTests.TestData;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Application.UnitTests.Submissions.UseCases;

public class GetSubmissionResultUseCaseTests
{
    private readonly ISubmissionRepository _repo = Substitute.For<ISubmissionRepository>();

    private GetSubmissionResultUseCase Build() => new(_repo);

    [Fact]
    public async Task Returns_Response_When_Found()
    {
        var submission = AppFixtures.Submission();
        _repo.GetByIdAsync(submission.Id, Arg.Any<CancellationToken>()).Returns(submission);
        var sut = Build();

        var result = await sut.ExecuteAsync(new GetSubmissionResultRequest(submission.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(submission.Id);
        result.Value.IsGraded.Should().BeFalse();
    }

    [Fact]
    public async Task Returns_NotFound_When_Missing()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((StudentSubmission?)null);
        var sut = Build();

        var result = await sut.ExecuteAsync(new GetSubmissionResultRequest(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("submission.not_found");
    }
}
