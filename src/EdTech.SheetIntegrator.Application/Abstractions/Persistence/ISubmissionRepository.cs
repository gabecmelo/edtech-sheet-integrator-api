using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Application.Abstractions.Persistence;

/// <summary>Read/write access to the StudentSubmission aggregate.</summary>
public interface ISubmissionRepository
{
    Task<StudentSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(StudentSubmission submission, CancellationToken cancellationToken);

    Task<IReadOnlyList<StudentSubmission>> ListByAssessmentAsync(
        Guid assessmentId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountByAssessmentAsync(Guid assessmentId, CancellationToken cancellationToken);
}
