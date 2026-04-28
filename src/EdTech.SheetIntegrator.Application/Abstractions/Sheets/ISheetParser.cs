namespace EdTech.SheetIntegrator.Application.Abstractions.Sheets;

/// <summary>One concrete parser per supported file format (xlsx, csv, ...).</summary>
public interface ISheetParser
{
    /// <summary>
    /// Whether this parser handles a given upload. Implementations should check the file
    /// extension and the optional content-type header.
    /// </summary>
    bool CanParse(string fileName, string? contentType);

    /// <summary>
    /// Read a sheet stream into a list of (question id, response) pairs. Throws
    /// <see cref="SheetParsingException"/> when the file is malformed.
    /// </summary>
    Task<IReadOnlyList<RawAnswer>> ParseAsync(Stream stream, CancellationToken cancellationToken);
}
