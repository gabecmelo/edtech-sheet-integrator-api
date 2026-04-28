namespace EdTech.SheetIntegrator.Application.Common;

/// <summary>Abstraction over the system clock so use cases stay deterministic in tests.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
