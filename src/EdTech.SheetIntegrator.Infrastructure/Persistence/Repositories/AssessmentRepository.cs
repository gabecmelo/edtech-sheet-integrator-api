using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Domain.Assessments;
using Microsoft.EntityFrameworkCore;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence.Repositories;

internal sealed class AssessmentRepository : IAssessmentRepository
{
    private readonly AppDbContext _context;

    public AssessmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Assessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Assessments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Assessment assessment, CancellationToken cancellationToken)
    {
        await _context.Assessments.AddAsync(assessment, cancellationToken);
    }
}
