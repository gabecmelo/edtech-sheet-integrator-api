namespace EdTech.SheetIntegrator.Application.Abstractions.Persistence;

/// <summary>Commit boundary. Use cases call this once after orchestrating repository writes.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
