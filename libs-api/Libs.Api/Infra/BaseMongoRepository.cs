using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class BaseMongoRepository<T>
{
    protected SortDefinitionBuilder<T> _sort = Builders<T>.Sort;
    protected FilterDefinitionBuilder<T> _filter = Builders<T>.Filter;
    protected UpdateDefinitionBuilder<T> _update = Builders<T>.Update;

    protected virtual UpdateDefinition<T> PrepareToUpdate(T newValue)
    {
        var updateDefinitionList = new List<UpdateDefinition<T>>();
        var propriedades = typeof(T).GetProperties();
        foreach (var property in propriedades)
        {
            var modifiedValue = property.GetValue(newValue);

            updateDefinitionList.Add(
                _update.Set(property.Name, modifiedValue)
            );
        }
        
        return _update.Combine(updateDefinitionList);
    }

}