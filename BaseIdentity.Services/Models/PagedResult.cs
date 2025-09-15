namespace BaseIdentity.Services.Models;

public class PagedResult<T>
{
    public const int DefaultPageSize = 20;

    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = DefaultPageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
