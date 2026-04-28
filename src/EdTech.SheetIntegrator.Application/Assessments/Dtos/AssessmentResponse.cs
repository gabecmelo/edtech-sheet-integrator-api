using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Application.Assessments.Dtos;

public sealed record QuestionResponse(
    string QuestionId,
    string Prompt,
    decimal Points,
    MatchMode MatchMode,
    decimal? NumericTolerance);

public sealed record AssessmentResponse(
    Guid Id,
    string Title,
    decimal MaxScore,
    DateTimeOffset CreatedAt,
    IReadOnlyList<QuestionResponse> Questions)
{
    public static AssessmentResponse From(Assessment assessment) =>
        new(
            assessment.Id,
            assessment.Title,
            assessment.MaxScore,
            assessment.CreatedAt,
            assessment.Questions
                .Select(q => new QuestionResponse(q.QuestionId, q.Prompt, q.Points, q.MatchMode, q.NumericTolerance))
                .ToList());
}
