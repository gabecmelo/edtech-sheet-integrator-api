using System.Text;
using ClosedXML.Excel;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;
using EdTech.SheetIntegrator.Infrastructure.Sheets;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.Sheets;

public class ClosedXmlSheetParserTests
{
    private readonly ClosedXmlSheetParser _parser = new();

    [Theory]
    [InlineData("alice.xlsx", null, true)]
    [InlineData("alice.XLSX", null, true)]
    [InlineData("alice.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
    [InlineData("alice.csv", "text/csv", false)]
    [InlineData("alice.txt", null, false)]
    public void CanParse_Recognizes_Xlsx_By_Extension_Or_ContentType(string name, string? contentType, bool expected)
    {
        _parser.CanParse(name, contentType).Should().Be(expected);
    }

    [Fact]
    public async Task ParseAsync_Reads_QuestionId_Response_Pairs_And_Skips_Header()
    {
        using var stream = BuildXlsxStream(
            ("QuestionId", "Response"),
            ("Q1", "Paris"),
            ("Q2", "3.14"),
            ("Q3", "Mitochondria"));

        var answers = await _parser.ParseAsync(stream, CancellationToken.None);

        answers.Should().HaveCount(3);
        answers[0].Should().Be(new RawAnswer("Q1", "Paris"));
        answers[1].Should().Be(new RawAnswer("Q2", "3.14"));
        answers[2].Should().Be(new RawAnswer("Q3", "Mitochondria"));
    }

    [Fact]
    public async Task ParseAsync_Skips_Rows_With_Blank_QuestionId()
    {
        using var stream = BuildXlsxStream(
            ("QuestionId", "Response"),
            ("Q1", "Paris"),
            ("", "ignored"),
            ("Q2", "3.14"));

        var answers = await _parser.ParseAsync(stream, CancellationToken.None);

        answers.Should().HaveCount(2);
        answers.Select(a => a.QuestionId).Should().ContainInOrder("Q1", "Q2");
    }

    [Fact]
    public async Task ParseAsync_Throws_SheetParsingException_On_Malformed_Workbook()
    {
        // A stream of plain text is not a valid xlsx (zip) file.
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not a workbook"));

        var act = () => _parser.ParseAsync(stream, CancellationToken.None);

        await act.Should().ThrowAsync<SheetParsingException>();
    }

    private static MemoryStream BuildXlsxStream(params (string Col1, string Col2)[] rows)
    {
        var stream = new MemoryStream();
        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.AddWorksheet("Answers");
            for (var i = 0; i < rows.Length; i++)
            {
                ws.Cell(i + 1, 1).Value = rows[i].Col1;
                ws.Cell(i + 1, 2).Value = rows[i].Col2;
            }
            workbook.SaveAs(stream);
        }
        stream.Position = 0;
        return stream;
    }
}
