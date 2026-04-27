using System.Globalization;
using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Domain.Assessments;

/// <summary>
/// Immutable value object describing a single question and the rule for matching responses against it.
/// Equality is structural (record).
/// </summary>
public sealed record Question
{
    public string QuestionId { get; }

    public string Prompt { get; }

    public string CorrectAnswer { get; }

    public decimal Points { get; }

    public MatchMode MatchMode { get; }

    public decimal? NumericTolerance { get; }

    public Question(
        string questionId,
        string prompt,
        string correctAnswer,
        decimal points,
        MatchMode matchMode,
        decimal? numericTolerance = null)
    {
        if (string.IsNullOrWhiteSpace(questionId))
        {
            throw new InvalidQuestionConfigurationException("Question id is required.");
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new InvalidQuestionConfigurationException(
                $"Prompt is required for question '{questionId}'.");
        }

        if (correctAnswer is null)
        {
            throw new InvalidQuestionConfigurationException(
                $"Correct answer is required for question '{questionId}'.");
        }

        if (points <= 0m)
        {
            throw new InvalidQuestionConfigurationException(
                $"Points for question '{questionId}' must be greater than zero.");
        }

        if (!Enum.IsDefined(matchMode))
        {
            throw new InvalidQuestionConfigurationException(
                $"Unknown match mode for question '{questionId}'.");
        }

        if (matchMode == MatchMode.Numeric)
        {
            if (numericTolerance is null || numericTolerance < 0m)
            {
                throw new InvalidQuestionConfigurationException(
                    $"Numeric tolerance for question '{questionId}' must be a non-negative number.");
            }

            if (!decimal.TryParse(correctAnswer, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                throw new InvalidQuestionConfigurationException(
                    $"Correct answer for numeric question '{questionId}' must be a number.");
            }
        }
        else if (numericTolerance is not null)
        {
            throw new InvalidQuestionConfigurationException(
                $"Numeric tolerance is only valid for Numeric match mode (question '{questionId}').");
        }

        QuestionId = questionId;
        Prompt = prompt;
        CorrectAnswer = correctAnswer;
        Points = points;
        MatchMode = matchMode;
        NumericTolerance = numericTolerance;
    }

    /// <summary>True when <paramref name="response"/> satisfies this question's match rule.</summary>
    public bool Matches(string? response)
    {
        if (response is null)
        {
            return false;
        }

        return MatchMode switch
        {
            MatchMode.Exact => string.Equals(CorrectAnswer, response, StringComparison.Ordinal),
            MatchMode.CaseInsensitive => string.Equals(
                CorrectAnswer.Trim(),
                response.Trim(),
                StringComparison.OrdinalIgnoreCase),
            MatchMode.Numeric => MatchesNumeric(response),
            _ => false,
        };
    }

    private bool MatchesNumeric(string response)
    {
        if (!decimal.TryParse(response, NumberStyles.Number, CultureInfo.InvariantCulture, out var actual))
        {
            return false;
        }

        var expected = decimal.Parse(CorrectAnswer, NumberStyles.Number, CultureInfo.InvariantCulture);
        var tolerance = NumericTolerance ?? 0m;
        return Math.Abs(actual - expected) <= tolerance;
    }
}
