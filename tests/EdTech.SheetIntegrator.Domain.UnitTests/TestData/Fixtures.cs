using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Domain.UnitTests.TestData;

/// <summary>Helpers that produce valid domain instances so tests can focus on the behavior under test.</summary>
internal static class Fixtures
{
    public static readonly DateTimeOffset Now = new(2026, 4, 27, 12, 0, 0, TimeSpan.Zero);

    public static Question ExactQuestion(string id = "Q1", string answer = "Paris", decimal points = 1m) =>
        new(id, "Capital of France?", answer, points, MatchMode.Exact);

    public static Question CaseInsensitiveQuestion(string id = "Q2", string answer = "Mitochondria", decimal points = 2m) =>
        new(id, "Powerhouse of the cell?", answer, points, MatchMode.CaseInsensitive);

    public static Question NumericQuestion(
        string id = "Q3",
        string answer = "3.14",
        decimal points = 5m,
        decimal tolerance = 0.01m) =>
        new(id, "Approximate value of pi?", answer, points, MatchMode.Numeric, tolerance);

    public static Assessment Assessment(
        Guid? id = null,
        string title = "Sample Assessment",
        IEnumerable<Question>? questions = null,
        DateTimeOffset? createdAt = null) =>
        new(
            id ?? Guid.NewGuid(),
            title,
            questions ?? [ExactQuestion(), CaseInsensitiveQuestion(), NumericQuestion()],
            createdAt ?? Now);

    public static StudentSubmission Submission(
        Guid? id = null,
        Guid? assessmentId = null,
        string studentIdentifier = "alice@example.com",
        IEnumerable<Answer>? answers = null,
        string sourceFileName = "alice.xlsx",
        DateTimeOffset? submittedAt = null) =>
        new(
            id ?? Guid.NewGuid(),
            assessmentId ?? Guid.NewGuid(),
            studentIdentifier,
            answers ?? [new Answer("Q1", "Paris")],
            sourceFileName,
            submittedAt ?? Now);
}
