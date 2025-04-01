using MongoDB.Bson;
using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class BaseMongoRepository<T>
{
    protected SortDefinitionBuilder<T> _sortBuilder = Builders<T>.Sort;
    protected FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;
    protected UpdateDefinitionBuilder<T> _updateBuilder = Builders<T>.Update;
    protected RenderArgs<T> _renderArgs;
    protected IMongoCollection<T> Collection;

    protected BaseMongoRepository(IMongoCollection<T> collection)
    {
        Collection = collection;
        _renderArgs = new RenderArgs<T>(Collection.DocumentSerializer, Collection.Settings.SerializerRegistry);
    }

    protected virtual UpdateDefinition<T> PrepareToUpdate(T newValue)
    {
        var updateDefinitionList = new List<UpdateDefinition<T>>();
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var modifiedValue = property.GetValue(newValue);

            updateDefinitionList.Add(
                _updateBuilder.Set(property.Name, modifiedValue)
            );
        }

        return _updateBuilder.Combine(updateDefinitionList);
    }

    public virtual async Task AddAsync(T entity)
    {
        await Collection.InsertOneAsync(entity);
    }

    protected virtual async Task UpdateAsync(T entity, FilterDefinition<T> filterById)
    {
        var update = PrepareToUpdate(entity);
        await Collection.UpdateOneAsync(filterById, update);
    }
    protected virtual BsonDocument BuildEqualFilter(string propName, object propValue)
    {
        var filter = _filterBuilder.Eq(propName, propValue);

        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }

    protected virtual FilterDefinition<T> BuildDateFilter(FilterDefinition<T> filter, string propName, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue)
            filter &= _filterBuilder.Gte(propName, startDate.Value.Date);

        if(endDate.HasValue)
            filter &= _filterBuilder.Lt(propName, startDate.Value.AddDays(1));

        return filter;
    }

    protected virtual BsonDocument BuildDateFilter(string propName, DateTime? startDate, DateTime? endDate)
    {
        var filter = _filterBuilder.Empty;

        if (startDate.HasValue)
            filter &= _filterBuilder.Gte(propName, startDate.Value.Date);

        if ( endDate.HasValue)
            filter &= _filterBuilder.Lte(propName, endDate.Value.Date);

        var bsonFilter = filter.Render(_renderArgs).ToBsonDocument();
        return new BsonDocument("$match", bsonFilter);
    }
}