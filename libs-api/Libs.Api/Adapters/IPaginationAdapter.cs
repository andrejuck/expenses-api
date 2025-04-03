using Libs.Api.Models;

namespace Libs.Api.Adapters;

public interface IPaginationAdapter
{
    PagedResponse<T> ConvertToResponse<T>(PagedRequest request, long totalItems, IEnumerable<T> values);
}