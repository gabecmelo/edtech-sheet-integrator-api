using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Domain.Submissions;
using Microsoft.EntityFrameworkCore;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence.Repositories;

internal sealed class SubmissionRepository : ISubmissionRepository
{
    private readonly AppDbContext _context;

    public SubmissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<StudentSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Submissions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(StudentSubmission submission, CancellationToken cancellationToken)
    {
        await _context.Submissions.AddAsync(submission, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentSubmission>> ListByAssessmentAsync(
        Guid assessmentId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Submissions
            .Where(s => s.AssessmentId == assessmentId)
            .OrderByDescending(s => s.SubmittedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows;
    }

    public Task<int> CountByAssessmentAsync(Guid assessmentId, CancellationToken cancellationToken) =>
        _context.Submissions.CountAsync(s => s.AssessmentId == assessmentId, cancellationToken);
}
