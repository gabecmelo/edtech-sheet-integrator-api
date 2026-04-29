using System.Net.Http.Headers;
using System.Text;
using ClosedXML.Excel;

namespace EdTech.SheetIntegrator.Api.IntegrationTests.Helpers;

/// <summary>
/// Builds in-memory CSV and XLSX byte arrays to use as file upload fixtures,
/// without touching the file system.
/// </summary>
internal static class FileFixtures
{
    // ── CSV ──────────────────────────────────────────────────────────────────

    public static byte[] BuildCsv(params (string QuestionId, string Response)[] rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("QuestionId,Response");
        foreach (var (q, r) in rows)
            sb.AppendLine(q + "," + r);

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static StreamContent AsCsvContent(byte[] csvBytes, string fileName = "answers.csv")
    {
        var content = new StreamContent(new MemoryStream(csvBytes));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = fileName,
        };
        return content;
    }

    // ── XLSX ─────────────────────────────────────────────────────────────────

    public static byte[] BuildXlsx(params (string QuestionId, string Response)[] rows)
    {
        using var stream = new MemoryStream();
        using (var wb = new XLWorkbook())
        {
            var ws = wb.AddWorksheet("Answers");
            ws.Cell(1, 1).Value = "QuestionId";
            ws.Cell(1, 2).Value = "Response";

            for (var i = 0; i < rows.Length; i++)
            {
                ws.Cell(i + 2, 1).Value = rows[i].QuestionId;
                ws.Cell(i + 2, 2).Value = rows[i].Response;
            }

            wb.SaveAs(stream);
        }

        return stream.ToArray();
    }

    public static StreamContent AsXlsxContent(byte[] xlsxBytes, string fileName = "answers.xlsx")
    {
        var content = new StreamContent(new MemoryStream(xlsxBytes));
        content.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = fileName,
        };
        return content;
    }

    // ── Shared helper ─────────────────────────────────────────────────────────

    /// <summary>Wraps a raw <paramref name="content"/> as an arbitrary file with the given name.</summary>
    public static StreamContent AsRawContent(byte[] bytes, string fileName, string mediaType)
    {
        var content = new StreamContent(new MemoryStream(bytes));
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = fileName,
        };
        return content;
    }
}
