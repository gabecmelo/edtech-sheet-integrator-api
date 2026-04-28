using EdTech.SheetIntegrator.Domain.Exceptions;

namespace EdTech.SheetIntegrator.Domain.Grading;

/// <summary>
/// The result of grading a single question on a submission. Immutable value object
/// produced by <see cref="Assessments.Assessment.Grade"/>.
/// </summary>
public sealed record QuestionOutcome
{
    public string QuestionId { get; init; } = null!;

    public bool IsCorrect { get; init; }

    public decimal EarnedPoints { get; init; }

    public decimal MaxPoints { get; init; }

    // For System.Text.Json hydration; trusts persisted data.
    private QuestionOutcome()
    {
    }

    public QuestionOutcome(string questionId, bool isCorrect, decimal earnedPoints, decimal maxPoints)
    {
        if (string.IsNullOrWhiteSpace(questionId))
        {
            throw new DomainException("QuestionOutcome must reference a question id.");
        }

        if (maxPoints <= 0m)
        {
            throw new DomainException("QuestionOutcome max points must be greater than zero.");
        }

        if (earnedPoints < 0m)
        {
            throw new DomainException("QuestionOutcome earned points cannot be negative.");
        }

        if (earnedPoints > maxPoints)
        {
            throw new DomainException("QuestionOutcome earned points cannot exceed max points.");
        }

        QuestionId = questionId;
        IsCorrect = isCorrect;
        EarnedPoints = earnedPoints;
        MaxPoints = maxPoints;
    }
}
