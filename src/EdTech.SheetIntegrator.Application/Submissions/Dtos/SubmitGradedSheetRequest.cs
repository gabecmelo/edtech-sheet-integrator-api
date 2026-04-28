namespace EdTech.SheetIntegrator.Application.Submissions.Dtos;

/// <summary>
/// Inbound DTO for the core grading endpoint. <see cref="FileStream"/> is owned by the caller —
/// the use case reads it but does not dispose it.
/// </summary>
public sealed record SubmitGradedSheetRequest(
    Guid AssessmentId,
    string StudentIdentifier,
    string SourceFileName,
    string? ContentType,
    Stream FileStream);
