namespace Expenses.Domain.DataContracts.Generics;

public interface IBaseRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
}