using MongoDB.Bson;

namespace Expenses.Domain.DataContracts.Generics;

public interface IBaseEntityRepository<T> : IBaseRepository<T>
{
    Task<T> FindByIdAsync(Guid id, Guid userId);
}