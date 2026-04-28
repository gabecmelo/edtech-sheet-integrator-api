namespace EdTech.SheetIntegrator.Application.Abstractions.Sheets;

/// <summary>Raised by an <see cref="ISheetParser"/> when the sheet content is malformed or unreadable.</summary>
public sealed class SheetParsingException : Exception
{
    public SheetParsingException(string message)
        : base(message)
    {
    }

    public SheetParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
