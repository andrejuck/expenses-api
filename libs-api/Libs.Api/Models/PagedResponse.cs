namespace Libs.Api.Models;

public class PagedResponse<T>
{
    public long TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<T> Result { get; set; }
}