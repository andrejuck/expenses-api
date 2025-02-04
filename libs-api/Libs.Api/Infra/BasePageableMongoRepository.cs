using Libs.Api.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Libs.Api.Infra;

public abstract class BasePageableMongoRepository<T> : BaseMongoRepository<T>
{
    private RenderArgs<T> _renderArgs;

    protected BasePageableMongoRepository(IMongoCollection<T> collection)
        : base(collection)
    {
        _renderArgs = new RenderArgs<T>(Collection.DocumentSerializer, Collection.Settings.SerializerRegistry);
    }

    [Obsolete]
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

    protected virtual async Task<List<TResponse>> GetAllPagedAsync<TResponse>(PagedRequest request, BsonDocument[] aggregationPipeline)
    {
        aggregationPipeline = aggregationPipeline
            .Append(BuildCurrentPage(request))
            .Append(BuildPageSize(request))
            .ToArray();

        return await Collection
                .Aggregate<TResponse>(aggregationPipeline)
                .ToListAsync();
    }

    protected virtual async Task<long> GetAllCountAsync(FilterDefinition<T> filter)
        => await Collection.Find(filter).CountDocumentsAsync();

    [Obsolete]
    protected virtual async Task<long> GetAllCountAsync(PagedRequest request, SortDefinition<T> sortDefinition)
    {
        IEnumerable<FilterDefinition<T>> dynamicFilters = null;
        var userQuery = DefineFilters(request, ref sortDefinition, ref dynamicFilters);

        return await userQuery.CountDocumentsAsync();
    }

    protected virtual BsonDocument BuildFilters<TSearchParams>(TSearchParams request)
    {
        var filter = _filterBuilder.Empty;
        filter = DefineFilters(request, filter);

        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }

    protected virtual FilterDefinition<T> DefineFilters<TSearchParams>(TSearchParams request, FilterDefinition<T> filter)
    {
        if (request != null)
        {
            foreach (var property in typeof(TSearchParams).GetProperties())
            {
                var propValue = property.GetValue(request);
                var propType = property.PropertyType;

                if (propValue == null || propValue == default)
                    continue;

                if (propType == typeof(string))
                {
                    filter &= _filterBuilder.Regex(property.Name, new BsonRegularExpression(propValue.ToString(), "i"));
                    continue;
                }

                if (propType == typeof(List<string>))
                {
                    filter &= _filterBuilder.In(property.Name, (List<string>)propValue);
                    continue;
                }

                if (propType == typeof(Guid?) || propType == typeof(Guid))
                {
                    filter &= _filterBuilder.Eq(property.Name, (Guid)propValue);
                    continue;
                }
            }
        }

        return filter;
    }
    protected virtual BsonDocument BuildEqualFilter(string propName, object propValue)
    {
        var filter = _filterBuilder.Eq(propName, propValue);

        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }

    protected virtual BsonDocument BuildDateFilter(string propName, DateTime? date)
    {
        var filter = _filterBuilder.Empty;

        if (date.HasValue)
            filter = _filterBuilder.Gte(propName, date.Value.Date) & _filterBuilder.Lt(propName, date.Value.AddDays(1));


        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }

    protected virtual FilterDefinition<T> BuildDateFilter(FilterDefinition<T> filter, string propName, DateTime? date)
    {
        if (date.HasValue)
            filter &= _filterBuilder.Gte(propName, date.Value.Date) & _filterBuilder.Lt(propName, date.Value.AddDays(1));

        return filter;
    }

    protected virtual BsonDocument BuildDateFilter(string propName, DateTime? startDate, DateTime? endDate)
    {
        var filter = _filterBuilder.Empty;

        if (startDate.HasValue && endDate.HasValue)
            filter = _filterBuilder.Gte(propName, startDate.Value.Date) & _filterBuilder.Lt(propName, endDate.Value.Date);

        if (startDate.HasValue && !endDate.HasValue)
            filter = _filterBuilder.Gte(propName, startDate.Value.Date);

        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }

    protected virtual BsonDocument BuildSorting(PagedRequest request, ref SortDefinition<T> sortDefinition)
    {
        if (request.IsSorted)
        {
            if (Enum.TryParse<SortOrder>(request.SortOrder, out var sortOrder))
            {
                if (sortOrder == SortOrder.Ascending)
                {
                    sortDefinition = _sortBuilder.Ascending(request.SortKey);
                }
                else
                {
                    sortDefinition = _sortBuilder.Descending(request.SortKey);
                }
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

    [Obsolete]
    private IFindFluent<T, T> DefineFilters(PagedRequest request, ref SortDefinition<T> sortDefinition, ref IEnumerable<FilterDefinition<T>> dynamicFilters)
    {
        // if (request.Filters != null)
        // {
        //     dynamicFilters = request.Filters.Select(x => _filterBuilder.In(x.Key, x.Value));
        //     if (request.Filters.TryGetValue("UserId", out var userId))
        //     {
        //         dynamicFilters.Append(_filterBuilder.Eq("UserId", Guid.Parse(userId)));
        //     }
        // }

        if (request.IsSorted)
        {
            if (Enum.TryParse<SortOrder>(request.SortOrder, out var sortOrder))
            {
                if (sortOrder == SortOrder.Ascending)
                {
                    sortDefinition = _sortBuilder.Ascending(request.SortKey);
                }
                else
                {
                    sortDefinition = _sortBuilder.Descending(request.SortKey);
                }
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