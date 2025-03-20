using Libs.Api.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Libs.Api.Infra;

public abstract class BasePageableMongoRepository<T> : BaseMongoRepository<T>
{

    protected BasePageableMongoRepository(IMongoCollection<T> collection)
        : base(collection)
    {
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

}