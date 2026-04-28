using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Application.Submissions.Dtos;

public sealed record QuestionOutcomeResponse(
    string QuestionId,
    bool IsCorrect,
    decimal EarnedPoints,
    decimal MaxPoints);

public sealed record SubmissionResultResponse(
    Guid Id,
    Guid AssessmentId,
    string StudentIdentifier,
    string SourceFileName,
    DateTimeOffset SubmittedAt,
    bool IsGraded,
    decimal? Earned,
    decimal? Total,
    decimal? Percentage,
    DateTimeOffset? GradedAt,
    IReadOnlyList<QuestionOutcomeResponse> Outcomes)
{
    public static SubmissionResultResponse From(StudentSubmission submission)
    {
        var outcomes = submission.Result?.Outcomes
            .Select(o => new QuestionOutcomeResponse(o.QuestionId, o.IsCorrect, o.EarnedPoints, o.MaxPoints))
            .ToList() ?? [];

        return new SubmissionResultResponse(
            submission.Id,
            submission.AssessmentId,
            submission.StudentIdentifier,
            submission.SourceFileName,
            submission.SubmittedAt,
            submission.IsGraded,
            submission.Result?.Score.Earned,
            submission.Result?.Score.Total,
            submission.Result?.Score.Percentage,
            submission.Result?.GradedAt,
            outcomes);
    }
}
