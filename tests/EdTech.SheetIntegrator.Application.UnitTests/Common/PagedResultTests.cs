using EdTech.SheetIntegrator.Application.Common;

namespace EdTech.SheetIntegrator.Application.UnitTests.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 20, 0)]   // empty
    [InlineData(20, 20, 1)]  // exactly one page
    [InlineData(21, 20, 2)]  // partial second page
    [InlineData(40, 20, 2)]  // exactly two pages
    [InlineData(41, 20, 3)]  // partial third page
    public void TotalPages_Is_Ceiling_Of_TotalCount_Over_PageSize(int total, int pageSize, int expected)
    {
        var paged = new PagedResult<int>([], 1, pageSize, total);

        paged.TotalPages.Should().Be(expected);
    }

    [Fact]
    public void TotalPages_Is_Zero_When_PageSize_Is_Zero()
    {
        var paged = new PagedResult<int>([], 1, 0, 50);

        paged.TotalPages.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 20, 50, true)]   // 3 pages, on page 1
    [InlineData(2, 20, 50, true)]   // on page 2 of 3
    [InlineData(3, 20, 50, false)]  // last page
    [InlineData(1, 20, 0, false)]   // empty
    public void HasNextPage_Reflects_Current_Vs_Total(int page, int pageSize, int total, bool expected)
    {
        var paged = new PagedResult<int>([], page, pageSize, total);

        paged.HasNextPage.Should().Be(expected);
    }
}
