using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Infra;
using Libs.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories;

public class ExpenseRepository : BasePageableMongoRepository<Expense>, IExpenseRepository
{
    private DBContext _dbContext;

    public ExpenseRepository(DBContext dbContext)
        : base(dbContext.Expenses)
    {
        _dbContext = dbContext;
    }

    private FilterDefinition<Expense> IdFilter(Guid id)
    {
        return _filterBuilder.Eq(x => x.Id, id);
    }

    public async Task<Expense> FindByIdAsync(Guid id, Guid userId)
    {
        var filter = _filterBuilder.And(IdFilter(id), _filterBuilder.Eq(x => x.UserId, userId));
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Expense entity)
    {
        var filter = IdFilter(entity.Id);
        await base.UpdateAsync(entity, filter);
    }

    public Task<long> GetAllCountAsync(PagedRequest request)
    {
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        return base.GetAllCountAsync(request, sortDefinition);
    }

    public async Task<List<TResponse>> GetAllPagedAsync<TResponse>(PagedRequest request)
    {
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        var aggregatedBson = new BsonDocument[] {
            BuildFilters(request),
            BuildAggregation(_dbContext.PaymentMethods.CollectionNamespace.CollectionName, nameof(Expense.PaymentMethodId), nameof(PaymentMethod)),
            BuildFlatChildAggregation(nameof(PaymentMethod)),
            BuildSorting(request, ref sortDefinition)
        };

        var pagedResult = await base.GetAllPagedAsync<TResponse>(request, aggregatedBson);

        return pagedResult;
    }
}