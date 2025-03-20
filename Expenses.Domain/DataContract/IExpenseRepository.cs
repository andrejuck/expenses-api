using Expenses.Domain.DataContracts.Generics;
using Expenses.Domain.Models;
using Libs.Api.Models;

namespace Expenses.Domain.DataContracts;

public interface IExpenseRepository : IBaseEntityRepository<Expense>
{
    Task<long> GetAllCountAsync(ExpenseSearchParam searchParams, Guid userId);
    Task<List<TResponse>> GetAllPagedAsync<TResponse>(ExpenseSearchParam searchParam, PagedRequest request, Guid userId);
    Task<List<TResponse>> GetAllGroupedPagedAsync<TResponse>(ExpenseSearchParam searchParam, PagedRequest request, Guid userId);
    Task<List<string>> GetAllUserCategories(Guid userId);
}