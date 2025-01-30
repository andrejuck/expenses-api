using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class BaseMongoRepository<T>
{
    protected SortDefinitionBuilder<T> _sort = Builders<T>.Sort;
    protected FilterDefinitionBuilder<T> _filter = Builders<T>.Filter;
    protected UpdateDefinitionBuilder<T> _update = Builders<T>.Update;
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
                _update.Set(property.Name, modifiedValue)
            );
        }

        return _update.Combine(updateDefinitionList);
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