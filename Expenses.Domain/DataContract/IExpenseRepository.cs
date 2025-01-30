using Expenses.Domain.DataContracts.Generics;
using Expenses.Domain.Models;
using Libs.Api.Models;

namespace Expenses.Domain.DataContracts;

public interface IExpenseRepository : IBaseEntityRepository<Expense>
{
    Task<long> GetAllCountAsync(PagedRequest request);
    Task<IEnumerable<Expense>> GetAllPagedAsync(PagedRequest request);
}