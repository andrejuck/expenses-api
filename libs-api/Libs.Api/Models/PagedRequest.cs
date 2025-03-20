namespace Libs.Api.Models;

public class PagedRequest
{
    private int _pageSize;
    private int _currentPage;

    public int CurrentPage
    {
        get
        {
            if (_currentPage == 0)
                return 1;

            return _currentPage;
        }
        set => _currentPage = value;
    }
    public int PageSize
    {
        get
        {
            if (_pageSize > 50)
                return 50;

            if (_pageSize == 0)
                return 10;

            return _pageSize;
        }
        set => _pageSize = value;
    }

    public bool IsSorted { get; set; } = false;
    public string SortKey { get; set; }

    private string _sortOrder;
    public string SortOrder
    {
        get
        {
            if(string.IsNullOrEmpty(_sortOrder)) return "Ascending";
            
            return _sortOrder;
        }
        set { _sortOrder = value; }
    }
}

