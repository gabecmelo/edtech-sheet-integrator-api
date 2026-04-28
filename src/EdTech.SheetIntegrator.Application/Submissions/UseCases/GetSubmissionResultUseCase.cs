using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;

namespace EdTech.SheetIntegrator.Application.Submissions.UseCases;

public sealed class GetSubmissionResultUseCase : IUseCase<GetSubmissionResultRequest, SubmissionResultResponse>
{
    private readonly ISubmissionRepository _submissions;

    public GetSubmissionResultUseCase(ISubmissionRepository submissions)
    {
        _submissions = submissions;
    }

    public async Task<Result<SubmissionResultResponse>> ExecuteAsync(
        GetSubmissionResultRequest input,
        CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(input.Id, cancellationToken);
        if (submission is null)
        {
            return Errors.Submission.NotFound;
        }

        return SubmissionResultResponse.From(submission);
    }
}
