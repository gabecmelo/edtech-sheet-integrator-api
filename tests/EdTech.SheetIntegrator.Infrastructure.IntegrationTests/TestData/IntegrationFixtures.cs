using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.TestData;

internal static class IntegrationFixtures
{
    public static readonly DateTimeOffset Now = new(2026, 4, 28, 12, 0, 0, TimeSpan.Zero);

    public static Assessment AssessmentWithTwoQuestions(Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            "Sample quiz",
            [
                new Question("Q1", "Capital of France?", "Paris", 1m, MatchMode.Exact),
                new Question("Q2", "Pi to 2 dp?", "3.14", 5m, MatchMode.Numeric, numericTolerance: 0.01m),
            ],
            Now);

    public static StudentSubmission UngradedSubmission(Guid assessmentId, string studentId = "alice@x.com") =>
        new(
            Guid.NewGuid(),
            assessmentId,
            studentId,
            [new Answer("Q1", "Paris"), new Answer("Q2", "3.14")],
            "alice.xlsx",
            Now);

    public static StudentSubmission GradedSubmission(Assessment assessment, string studentId = "alice@x.com")
    {
        var submission = UngradedSubmission(assessment.Id, studentId);
        var result = assessment.Grade(submission.Answers, Now.AddMinutes(1));
        submission.AttachResult(result);
        return submission;
    }
}
