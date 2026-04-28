using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Application.Common;

namespace EdTech.SheetIntegrator.Application.Assessments.UseCases;

public sealed class GetAssessmentByIdUseCase : IUseCase<GetAssessmentByIdRequest, AssessmentResponse>
{
    private readonly IAssessmentRepository _repository;

    public GetAssessmentByIdUseCase(IAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AssessmentResponse>> ExecuteAsync(
        GetAssessmentByIdRequest input,
        CancellationToken cancellationToken)
    {
        var assessment = await _repository.GetByIdAsync(input.Id, cancellationToken);
        if (assessment is null)
        {
            return Errors.Assessment.NotFound;
        }

        return AssessmentResponse.From(assessment);
    }
}
