using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Infra;
using Libs.Api.Models;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories;

public class ExpenseRepository : BasePageableMongoRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(DBContext dbContext)
        : base(dbContext.Expenses)
    {
    }

    private FilterDefinition<Expense> IdFilter(Guid id)
    {
        return _filter.Eq(x => x.Id, id);
    }

    public async Task<Expense> FindByIdAsync(Guid id, Guid userId)
    {
        var filter = _filter.And(IdFilter(id), _filter.Eq(x => x.UserId, userId));
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Expense entity)
    {
        var filter = IdFilter(entity.Id);
        await base.UpdateAsync(entity, filter);
    }

    public Task<long> GetAllCountAsync(PagedRequest request)
    {
        var sortDefinition = _sort.Descending(x => x.TransactionDate);
        return base.GetAllCountAsync(request, sortDefinition);
    }

    public async Task<IEnumerable<Expense>> GetAllPagedAsync(PagedRequest request)
    {
        var sortDefinition = _sort.Descending(x => x.TransactionDate);
        return await base.GetAllPagedAsync(request, sortDefinition);
    }
}