using EdTech.SheetIntegrator.Application.Abstractions.Sheets;

namespace EdTech.SheetIntegrator.Infrastructure.Sheets;

/// <summary>
/// Resolves an <see cref="ISheetParser"/> by asking each registered parser in turn whether it
/// claims the upload. The first match wins. Returns <see langword="null"/> when no parser claims
/// the file — the use case translates that into <c>submission.unsupported_file_type</c>.
/// </summary>
internal sealed class SheetParserFactory : ISheetParserFactory
{
    private readonly IReadOnlyList<ISheetParser> _parsers;

    public SheetParserFactory(IEnumerable<ISheetParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    public ISheetParser? Resolve(string fileName, string? contentType) =>
        _parsers.FirstOrDefault(p => p.CanParse(fileName, contentType));
}
