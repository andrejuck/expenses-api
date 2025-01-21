namespace Expenses.Domain.DataContracts;

public interface IBaseRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task<T> GetByIdAsync(Guid id);
}