using EdTech.SheetIntegrator.Domain.Common;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Domain.Submissions;

/// <summary>
/// Domain event raised when a submission transitions from "submitted" to "graded".
/// Captured for downstream side-effects (notifications, analytics) — not dispatched in v1.
/// </summary>
public sealed record SubmissionGraded(
    Guid SubmissionId,
    Guid AssessmentId,
    Score Score,
    DateTimeOffset OccurredAt) : IDomainEvent;
