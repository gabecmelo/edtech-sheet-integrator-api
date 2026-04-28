using EdTech.SheetIntegrator.Application.Common;

namespace EdTech.SheetIntegrator.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
