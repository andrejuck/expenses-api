using Libs.Api.Models;
using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models;

namespace Transactions.Domain.DataContracts;

public interface IExpenseRepository : IBaseEntityRepository<Expense>
{
    Task<long> GetAllCountAsync(ExpenseSearchParam searchParams, Guid userId);
    Task<List<TResponse>> GetAllPagedAsync<TResponse>(ExpenseSearchParam searchParam, PagedRequest request, Guid userId);
    Task<List<TResponse>> GetAllGroupedPagedAsync<TResponse>(ExpenseSearchParam searchParam, PagedRequest request, Guid userId);
    Task<List<string>> GetAllUserCategories(Guid userId);
    Task<List<TResponse>> GetAllAsync<TResponse>(ExpenseSearchParam searchParam, Guid userId);
}