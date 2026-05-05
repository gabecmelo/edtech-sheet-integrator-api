using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;

namespace EdTech.SheetIntegrator.Infrastructure.Sheets;

/// <summary>
/// Parses CSV uploads. Convention: column 1 = question id, column 2 = response, header row present.
/// Uses invariant culture so a Brazilian-Portuguese spreadsheet parses identically to a US one.
/// </summary>
internal sealed class CsvHelperSheetParser : ISheetParser
{
    private const string _csvExtension = ".csv";

    private static readonly HashSet<string> _csvContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/csv",
        "application/csv",
        "text/plain",
    };

    public bool CanParse(string fileName, string? contentType)
    {
        if (!string.IsNullOrEmpty(fileName) &&
            Path.GetExtension(fileName).Equals(_csvExtension, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return contentType is not null && _csvContentTypes.Contains(contentType);
    }

    public async Task<IReadOnlyList<RawAnswer>> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
        };

        var answers = new List<RawAnswer>();

        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync().ConfigureAwait(false);
            csv.ReadHeader();

            while (await csv.ReadAsync().ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var questionId = csv.GetField(0)?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(questionId))
                {
                    continue;
                }

                var response = csv.GetField(1) ?? string.Empty;
                answers.Add(new RawAnswer(questionId, response));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new SheetParsingException("Failed to read CSV file.", ex);
        }

        return answers;
    }
}
