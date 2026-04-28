namespace EdTech.SheetIntegrator.Application.Common;

/// <summary>
/// Single-method seam for an application use case. Each use case is one class implementing this
/// interface — keeps the dependency graph minimal and the seams explicit (no mediator runtime).
/// </summary>
public interface IUseCase<in TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
