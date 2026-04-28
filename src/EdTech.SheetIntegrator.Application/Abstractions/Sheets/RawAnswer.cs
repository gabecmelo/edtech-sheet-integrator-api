namespace EdTech.SheetIntegrator.Application.Abstractions.Sheets;

/// <summary>
/// A single (question id, response) pair as read from a sheet, before being mapped to a
/// <see cref="EdTech.SheetIntegrator.Domain.Submissions.Answer"/>. Allows parsers to live
/// in Infrastructure without depending on Domain types directly.
/// </summary>
public sealed record RawAnswer(string QuestionId, string Response);
