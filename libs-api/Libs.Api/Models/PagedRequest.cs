namespace Libs.Api.Models;

public class PagedRequest
{
    private int _pageSize;

    public int CurrentPage { get; set; }
    public int PageSize
    {
        get
        {
            if (_pageSize > 50)
                return 50;

            return _pageSize;
        }
        set => _pageSize = value;
    }

    public Dictionary<string, string> Filters { get; set; }
    public bool IsSorted { get; set; }
    public (string key, SortOrder order)? SortingOrder { get; set; }
}

