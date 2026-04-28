namespace EdTech.SheetIntegrator.Application.Common;

/// <summary>Generic page envelope returned by listing use cases.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasNextPage => Page < TotalPages;
}
