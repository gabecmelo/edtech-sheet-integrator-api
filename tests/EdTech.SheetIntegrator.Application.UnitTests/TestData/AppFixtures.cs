using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Application.UnitTests.TestData;

internal static class AppFixtures
{
    public static readonly DateTimeOffset Now = new(2026, 4, 28, 12, 0, 0, TimeSpan.Zero);

    public static QuestionInput ExactQuestionInput(
        string id = "Q1",
        string answer = "Paris",
        decimal points = 1m) =>
        new(id, "Capital of France?", answer, points, MatchMode.Exact, NumericTolerance: null);

    public static QuestionInput NumericQuestionInput(
        string id = "Q3",
        string answer = "3.14",
        decimal points = 5m,
        decimal tolerance = 0.01m) =>
        new(id, "Pi?", answer, points, MatchMode.Numeric, tolerance);

    public static CreateAssessmentRequest CreateAssessmentRequest(
        string title = "Quiz",
        IReadOnlyList<QuestionInput>? questions = null) =>
        new(title, questions ?? [ExactQuestionInput()]);

    public static Assessment Assessment(
        Guid? id = null,
        string title = "Quiz",
        IEnumerable<Question>? questions = null) =>
        new(
            id ?? Guid.NewGuid(),
            title,
            questions ?? [new Question("Q1", "Capital of France?", "Paris", 1m, MatchMode.Exact)],
            Now);

    public static StudentSubmission Submission(
        Guid? id = null,
        Guid? assessmentId = null,
        string identifier = "alice@example.com",
        IEnumerable<Answer>? answers = null,
        string sourceFileName = "alice.xlsx") =>
        new(
            id ?? Guid.NewGuid(),
            assessmentId ?? Guid.NewGuid(),
            identifier,
            answers ?? [new Answer("Q1", "Paris")],
            sourceFileName,
            Now);

    public static SubmitGradedSheetRequest SubmitRequest(
        Guid assessmentId,
        Stream? stream = null,
        string sourceFileName = "alice.xlsx",
        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") =>
        new(
            assessmentId,
            "alice@example.com",
            sourceFileName,
            contentType,
            stream ?? new MemoryStream());
}
