namespace Expenses.Domain.DataContract;

public interface IBaseRepository<T> {

    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task<T> GetByIdAsync(Guid id);
}