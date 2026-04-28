using EdTech.SheetIntegrator.Application.Common;

namespace EdTech.SheetIntegrator.Application.UnitTests.TestData;

/// <summary>Deterministic <see cref="IClock"/> for tests.</summary>
internal sealed class FakeClock : IClock
{
    public FakeClock(DateTimeOffset now)
    {
        UtcNow = now;
    }

    public DateTimeOffset UtcNow { get; set; }
}
