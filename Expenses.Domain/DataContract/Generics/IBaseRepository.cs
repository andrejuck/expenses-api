namespace Expenses.Domain.DataContracts.Generics;

public interface IBaseRepository<T>
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
}