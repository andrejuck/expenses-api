using Libs.Api.Models;
using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class BasePageableMongoRepository<T> : BaseMongoRepository<T>
{
    protected BasePageableMongoRepository(IMongoCollection<T> collection)
        : base(collection)
    {
    }

    protected virtual async Task<IEnumerable<T>> GetAllPagedAsync(PagedRequest request, SortDefinition<T> sortDefinition)
    {
        IEnumerable<FilterDefinition<T>> dynamicFilters = null;
        var userQuery = DefineFilters(request, ref sortDefinition, ref dynamicFilters);

        return await userQuery
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Limit(request.PageSize)
                .Sort(sortDefinition)
                .ToListAsync();
    }

    protected virtual async Task<long> GetAllCountAsync(PagedRequest request, SortDefinition<T> sortDefinition)
    {
        IEnumerable<FilterDefinition<T>> dynamicFilters = null;
        var userQuery = DefineFilters(request, ref sortDefinition, ref dynamicFilters);

        return await userQuery.CountDocumentsAsync();
    }

    private IFindFluent<T, T> DefineFilters(PagedRequest request, ref SortDefinition<T> sortDefinition, ref IEnumerable<FilterDefinition<T>> dynamicFilters)
    {
        if (request.Filters != null)
        {
            dynamicFilters = request.Filters.Select(x => _filter.In(x.Key, x.Value));
        }

        if (request.IsSorted)
        {

            if (request.SortingOrder?.order == SortOrder.Ascending)
            {
                sortDefinition = _sort.Ascending(request.SortingOrder?.key);
            }
            else
            {
                sortDefinition = _sort.Descending(request.SortingOrder?.key);
            }
        }

        var userQuery = Collection.Find(_filter.Empty);

        if (dynamicFilters != null)
        {
            userQuery = Collection.Find(_filter.And(dynamicFilters));
        }

        return userQuery;
    }
}