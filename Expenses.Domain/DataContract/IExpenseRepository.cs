using Expenses.Domain.DataContracts.Generics;
using Expenses.Domain.Models;
using Libs.Api.Models;
using MongoDB.Bson;

namespace Expenses.Domain.DataContracts;

public interface IExpenseRepository : IBaseEntityRepository<Expense>
{
    Task<long> GetAllCountAsync(PagedRequest request);
    Task<List<TResponse>> GetAllPagedAsync<TResponse>(PagedRequest request);
}