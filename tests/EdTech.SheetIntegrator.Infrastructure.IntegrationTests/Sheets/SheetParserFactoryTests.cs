using EdTech.SheetIntegrator.Infrastructure.Sheets;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.Sheets;

public class SheetParserFactoryTests
{
    private readonly SheetParserFactory _factory = new(
        [new ClosedXmlSheetParser(), new CsvHelperSheetParser()]);

    [Theory]
    [InlineData("alice.xlsx", null)]
    [InlineData("alice.XLSX", null)]
    public void Resolves_Xlsx_To_ClosedXml_Parser(string name, string? contentType)
    {
        _factory.Resolve(name, contentType).Should().BeOfType<ClosedXmlSheetParser>();
    }

    [Theory]
    [InlineData("alice.csv", null)]
    [InlineData("alice.CSV", "text/csv")]
    public void Resolves_Csv_To_CsvHelper_Parser(string name, string? contentType)
    {
        _factory.Resolve(name, contentType).Should().BeOfType<CsvHelperSheetParser>();
    }

    [Fact]
    public void Returns_Null_When_No_Parser_Claims_The_File()
    {
        _factory.Resolve("alice.bin", "application/octet-stream-other").Should().BeNull();
    }
}
