using EdTech.SheetIntegrator.Domain.Assessments;

namespace EdTech.SheetIntegrator.Application.Abstractions.Persistence;

/// <summary>Read/write access to the Assessment aggregate. Returns Domain types, not IQueryable.</summary>
public interface IAssessmentRepository
{
    Task<Assessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Assessment assessment, CancellationToken cancellationToken);
}
