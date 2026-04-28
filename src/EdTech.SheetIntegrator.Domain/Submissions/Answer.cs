using EdTech.SheetIntegrator.Domain.Exceptions;

namespace EdTech.SheetIntegrator.Domain.Submissions;

/// <summary>
/// Immutable value object: a single response from a student to one question on the sheet.
/// Empty responses are valid (the student left the cell blank); only the question id is required.
/// </summary>
public sealed record Answer
{
    public string QuestionId { get; init; } = null!;

    public string Response { get; init; } = null!;

    // For EF Core / System.Text.Json hydration; trusts persisted data.
    private Answer()
    {
    }

    public Answer(string questionId, string response)
    {
        if (string.IsNullOrWhiteSpace(questionId))
        {
            throw new DomainException("Answer must reference a question id.");
        }

        QuestionId = questionId;
        Response = response ?? string.Empty;
    }
}
