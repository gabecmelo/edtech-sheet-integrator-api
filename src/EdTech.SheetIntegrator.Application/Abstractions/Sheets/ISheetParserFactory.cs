namespace EdTech.SheetIntegrator.Application.Abstractions.Sheets;

/// <summary>
/// Strategy resolver: picks the right <see cref="ISheetParser"/> for an incoming upload.
/// Returns null when no registered parser claims the file.
/// </summary>
public interface ISheetParserFactory
{
    ISheetParser? Resolve(string fileName, string? contentType);
}
