using Libs.Api.Models;
using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Domain.DataContracts;

public interface IExpenseRepository : IBaseEntityRepository<Transaction>
{
    Task<long> GetAllCountAsync(TransactionSearchParam searchParams, Guid userId);
    Task<List<TResponse>> GetAllPagedAsync<TResponse>(TransactionSearchParam searchParam, PagedRequest request, Guid userId);
    Task<List<TResponse>> GetAllGroupedPagedAsync<TResponse>(TransactionSearchParam searchParam, PagedRequest request, Guid userId);
    Task<List<string>> GetAllUserCategories(Guid userId);
    Task<List<TResponse>> GetAllAsync<TResponse>(TransactionSearchParam searchParam, Guid userId);
}