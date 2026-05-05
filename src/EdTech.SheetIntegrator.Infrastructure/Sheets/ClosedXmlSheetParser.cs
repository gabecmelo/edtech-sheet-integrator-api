using ClosedXML.Excel;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;

namespace EdTech.SheetIntegrator.Infrastructure.Sheets;

/// <summary>
/// Parses .xlsx workbooks. Convention: column A holds question ids, column B holds responses,
/// row 1 is a header row that is skipped. Reads the first non-empty worksheet.
/// </summary>
internal sealed class ClosedXmlSheetParser : ISheetParser
{
    private const string _xlsxExtension = ".xlsx";

    private static readonly HashSet<string> _xlsxContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel.sheet.macroenabled.12",
        "application/octet-stream",
    };

    public bool CanParse(string fileName, string? contentType)
    {
        if (!string.IsNullOrEmpty(fileName) &&
            Path.GetExtension(fileName).Equals(_xlsxExtension, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return contentType is not null && _xlsxContentTypes.Contains(contentType);
    }

    public Task<IReadOnlyList<RawAnswer>> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new SheetParsingException("Workbook contains no worksheets.");

            var answers = new List<RawAnswer>();
            var range = worksheet.RangeUsed();
            if (range is null)
            {
                return Task.FromResult<IReadOnlyList<RawAnswer>>(answers);
            }

            var rows = range.RowsUsed().Skip(1); // skip header
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var questionId = row.Cell(1).GetString().Trim();
                if (string.IsNullOrEmpty(questionId))
                {
                    continue;
                }

                var response = row.Cell(2).GetString();
                answers.Add(new RawAnswer(questionId, response));
            }

            return Task.FromResult<IReadOnlyList<RawAnswer>>(answers);
        }
        catch (Exception ex) when (ex is not SheetParsingException and not OperationCanceledException)
        {
            throw new SheetParsingException("Failed to read xlsx workbook.", ex);
        }
    }
}
