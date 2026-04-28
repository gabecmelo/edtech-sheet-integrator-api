namespace EdTech.SheetIntegrator.Application.Submissions.Dtos;

public sealed record ListSubmissionsRequest(Guid AssessmentId, int Page = 1, int PageSize = 20);
