using EdTech.SheetIntegrator.Domain.Exceptions;

namespace EdTech.SheetIntegrator.Domain.Grading;

/// <summary>
/// Snapshot of a graded submission: per-question outcomes, the rolled-up score, and when grading occurred.
/// Returned by <see cref="Assessments.Assessment.Grade"/> and attached to the submission.
/// </summary>
public sealed record GradingResult
{
    public Score Score { get; init; }

    public IReadOnlyList<QuestionOutcome> Outcomes { get; init; } = [];

    public DateTimeOffset GradedAt { get; init; }

    // For System.Text.Json hydration; trusts persisted data.
    private GradingResult()
    {
    }

    public GradingResult(Score score, IReadOnlyList<QuestionOutcome> outcomes, DateTimeOffset gradedAt)
    {
        if (outcomes is null || outcomes.Count == 0)
        {
            throw new DomainException("GradingResult must contain at least one outcome.");
        }

        var totalFromOutcomes = outcomes.Sum(o => o.MaxPoints);
        if (totalFromOutcomes != score.Total)
        {
            throw new DomainException("GradingResult outcome max points do not sum to the score total.");
        }

        var earnedFromOutcomes = outcomes.Sum(o => o.EarnedPoints);
        if (earnedFromOutcomes != score.Earned)
        {
            throw new DomainException("GradingResult outcome earned points do not sum to the score earned.");
        }

        Score = score;
        Outcomes = outcomes;
        GradedAt = gradedAt;
    }
}
