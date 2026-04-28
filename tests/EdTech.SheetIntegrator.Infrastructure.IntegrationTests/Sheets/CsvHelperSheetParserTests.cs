using System.Text;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;
using EdTech.SheetIntegrator.Infrastructure.Sheets;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.Sheets;

public class CsvHelperSheetParserTests
{
    private readonly CsvHelperSheetParser _parser = new();

    [Theory]
    [InlineData("alice.csv", null, true)]
    [InlineData("alice.CSV", null, true)]
    [InlineData("alice.csv", "text/csv", true)]
    [InlineData("alice.txt", "text/csv", true)]
    [InlineData("alice.xlsx", null, false)]
    public void CanParse_Recognizes_Csv_By_Extension_Or_ContentType(string name, string? contentType, bool expected)
    {
        _parser.CanParse(name, contentType).Should().Be(expected);
    }

    [Fact]
    public async Task ParseAsync_Reads_QuestionId_Response_Pairs_And_Skips_Header()
    {
        var csv = "QuestionId,Response\nQ1,Paris\nQ2,3.14\nQ3,Mitochondria\n";
        using var stream = ToStream(csv);

        var answers = await _parser.ParseAsync(stream, CancellationToken.None);

        answers.Should().HaveCount(3);
        answers.Should().ContainInOrder(
            new RawAnswer("Q1", "Paris"),
            new RawAnswer("Q2", "3.14"),
            new RawAnswer("Q3", "Mitochondria"));
    }

    [Fact]
    public async Task ParseAsync_Preserves_Decimal_Separator_With_Invariant_Culture()
    {
        // CSV produced with comma as the decimal separator (e.g. pt-BR Excel export). The parser
        // must preserve the raw text "3,14" verbatim — converting to "3.14" would silently change
        // grading semantics. Locale-stable parsing is part of the contract.
        var csv = "QuestionId,Response\nQ1,\"3,14\"\n";
        using var stream = ToStream(csv);

        var answers = await _parser.ParseAsync(stream, CancellationToken.None);

        answers.Should().ContainSingle();
        answers[0].Response.Should().Be("3,14");
    }

    [Fact]
    public async Task ParseAsync_Skips_Rows_With_Blank_QuestionId()
    {
        var csv = "QuestionId,Response\nQ1,Paris\n,ignored\nQ2,3.14\n";
        using var stream = ToStream(csv);

        var answers = await _parser.ParseAsync(stream, CancellationToken.None);

        answers.Should().HaveCount(2);
        answers.Select(a => a.QuestionId).Should().ContainInOrder("Q1", "Q2");
    }

    [Fact]
    public async Task ParseAsync_Returns_Empty_When_Only_Header_Present()
    {
        using var stream = ToStream("QuestionId,Response\n");

        var answers = await _parser.ParseAsync(stream, CancellationToken.None);

        answers.Should().BeEmpty();
    }

    private static MemoryStream ToStream(string content) => new(Encoding.UTF8.GetBytes(content));
}
