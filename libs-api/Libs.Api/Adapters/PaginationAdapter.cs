using Libs.Api.Models;

namespace Libs.Api.Adapters;
public class PaginationAdapter : IPaginationAdapter
{
    public PagedResponse<T> ConvertToResponse<T>(PagedRequest request, long totalItems, IEnumerable<T> values)
    {
        return new PagedResponse<T>
        {
            CurrentPage = request.CurrentPage,
            PageSize = request.PageSize,
            Result = values,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }
}