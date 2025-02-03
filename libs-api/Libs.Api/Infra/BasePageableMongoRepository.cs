using Libs.Api.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Libs.Api.Infra;

public abstract class BasePageableMongoRepository<T> : BaseMongoRepository<T>
{
    private RenderArgs<T> _renderArgs;

    protected BasePageableMongoRepository(IMongoCollection<T> collection)
        : base(collection)
    {
        _renderArgs = new RenderArgs<T>(Collection.DocumentSerializer, Collection.Settings.SerializerRegistry);
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

    protected virtual async Task<List<T>> GetAllPagedAsync(PagedRequest request, BsonDocument[] aggregationPipeline)
    {
        aggregationPipeline = aggregationPipeline
            .Append(BuildCurrentPage(request))
            .Append(BuildPageSize(request))
            .ToArray();

        return await Collection
                .Aggregate<T>(aggregationPipeline)
                .ToListAsync();
    }

    protected virtual async Task<long> GetAllCountAsync(PagedRequest request, SortDefinition<T> sortDefinition)
    {
        IEnumerable<FilterDefinition<T>> dynamicFilters = null;
        var userQuery = DefineFilters(request, ref sortDefinition, ref dynamicFilters);

        return await userQuery.CountDocumentsAsync();
    }

    protected virtual BsonDocument BuildFilters(PagedRequest request)
    {
        var filter = _filterBuilder.Empty;

        if (request.Filters != null)
        {
            foreach (var item in request.Filters)
            {
                if (request.Filters.TryGetValue("UserId", out var userId))
                {
                    filter &= _filterBuilder.Eq("UserId", Guid.Parse(userId));
                    continue;
                }

                filter &= _filterBuilder.In(item.Key, item.Value);
            }
        }

        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }

    protected virtual BsonDocument BuildSorting(PagedRequest request, ref SortDefinition<T> sortDefinition)
    {
        if (request.IsSorted)
        {
            if (request.SortingOrder?.order == SortOrder.Ascending)
            {
                sortDefinition = _sortBuilder.Ascending(request.SortingOrder?.key);
            }
            else
            {
                sortDefinition = _sortBuilder.Descending(request.SortingOrder?.key);
            }
        }

        var bsonSorting = sortDefinition.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$sort", bsonSorting);
    }

    protected virtual BsonDocument BuildAggregation(string collectionName, string relationFieldName, string resultTypeName)
    {
        return new BsonDocument("$lookup",
            new BsonDocument
            {
                { "from", collectionName},
                { "localField", relationFieldName },
                { "foreignField", "_id" },
                { "as", resultTypeName }
            }
        );
    }

    protected virtual BsonDocument BuildFlatChildAggregation(string resultTypeName) => 
        new BsonDocument("$unwind", $"${resultTypeName}");

    private BsonDocument BuildCurrentPage(PagedRequest request) =>
        new BsonDocument("$skip", (request.CurrentPage - 1) * request.PageSize);

    private BsonDocument BuildPageSize(PagedRequest request) =>
        new BsonDocument("$limit", request.PageSize);

    private IFindFluent<T, T> DefineFilters(PagedRequest request, ref SortDefinition<T> sortDefinition, ref IEnumerable<FilterDefinition<T>> dynamicFilters)
    {
        if (request.Filters != null)
        {
            dynamicFilters = request.Filters.Select(x => _filterBuilder.In(x.Key, x.Value));
            var test = request.Filters.Select(x => _filterBuilder.In(x.Key, x.Value).ToBsonDocument());
            if (request.Filters.TryGetValue("UserId", out var userId))
            {
                dynamicFilters.Append(_filterBuilder.Eq("UserId", Guid.Parse(userId)));
            }
        }

        if (request.IsSorted)
        {

            if (request.SortingOrder?.order == SortOrder.Ascending)
            {
                sortDefinition = _sortBuilder.Ascending(request.SortingOrder?.key);
            }
            else
            {
                sortDefinition = _sortBuilder.Descending(request.SortingOrder?.key);
            }
        }

        var userQuery = Collection.Find(_filterBuilder.Empty);

        if (dynamicFilters != null && dynamicFilters.Count() > 0)
        {
            userQuery = Collection.Find(_filterBuilder.And(dynamicFilters));
        }

        return userQuery;
    }

}