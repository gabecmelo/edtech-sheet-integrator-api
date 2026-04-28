using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Application.Assessments.UseCases;
using EdTech.SheetIntegrator.Application.UnitTests.TestData;
using EdTech.SheetIntegrator.Domain.Assessments;

namespace EdTech.SheetIntegrator.Application.UnitTests.Assessments.UseCases;

public class GetAssessmentByIdUseCaseTests
{
    private readonly IAssessmentRepository _repo = Substitute.For<IAssessmentRepository>();

    private GetAssessmentByIdUseCase Build() => new(_repo);

    [Fact]
    public async Task Returns_Response_When_Found()
    {
        var assessment = AppFixtures.Assessment();
        _repo.GetByIdAsync(assessment.Id, Arg.Any<CancellationToken>()).Returns(assessment);
        var sut = Build();

        var result = await sut.ExecuteAsync(new GetAssessmentByIdRequest(assessment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(assessment.Id);
    }

    [Fact]
    public async Task Returns_NotFound_When_Missing()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Assessment?)null);
        var sut = Build();

        var result = await sut.ExecuteAsync(new GetAssessmentByIdRequest(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("assessment.not_found");
    }
}
