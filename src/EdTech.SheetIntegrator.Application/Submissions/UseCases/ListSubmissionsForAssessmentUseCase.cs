using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using FluentValidation;

namespace EdTech.SheetIntegrator.Application.Submissions.UseCases;

public sealed class ListSubmissionsForAssessmentUseCase
    : IUseCase<ListSubmissionsRequest, PagedResult<SubmissionResultResponse>>
{
    private readonly ISubmissionRepository _submissions;
    private readonly IAssessmentRepository _assessments;
    private readonly IValidator<ListSubmissionsRequest> _validator;

    public ListSubmissionsForAssessmentUseCase(
        ISubmissionRepository submissions,
        IAssessmentRepository assessments,
        IValidator<ListSubmissionsRequest> validator)
    {
        _submissions = submissions;
        _assessments = assessments;
        _validator = validator;
    }

    public async Task<Result<PagedResult<SubmissionResultResponse>>> ExecuteAsync(
        ListSubmissionsRequest input,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Errors.Validation.Failed(validation.Errors.Select(e => e.ErrorMessage));
        }

        var assessment = await _assessments.GetByIdAsync(input.AssessmentId, cancellationToken);
        if (assessment is null)
        {
            return Errors.Assessment.NotFound;
        }

        var skip = (input.Page - 1) * input.PageSize;
        var items = await _submissions.ListByAssessmentAsync(input.AssessmentId, skip, input.PageSize, cancellationToken);
        var total = await _submissions.CountByAssessmentAsync(input.AssessmentId, cancellationToken);

        var responses = items.Select(SubmissionResultResponse.From).ToList();

        return new PagedResult<SubmissionResultResponse>(responses, input.Page, input.PageSize, total);
    }
}
