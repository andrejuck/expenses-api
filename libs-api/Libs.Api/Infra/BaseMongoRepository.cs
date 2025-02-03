using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class BaseMongoRepository<T>
{
    protected SortDefinitionBuilder<T> _sortBuilder = Builders<T>.Sort;
    protected FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;
    protected UpdateDefinitionBuilder<T> _updateBuilder = Builders<T>.Update;
    protected IMongoCollection<T> Collection;

    protected BaseMongoRepository(IMongoCollection<T> collection)
    {
        Collection = collection;
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

}