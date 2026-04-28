using EdTech.SheetIntegrator.Application.Abstractions.Persistence;

namespace EdTech.SheetIntegrator.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public EfUnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
