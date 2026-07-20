namespace GameHub.Application.Common.Pagination;

// A bounded page of results plus the metadata a client needs to render
// "Page 3 of 47". Reused by every list endpoint.
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
