using Expenses.Domain.DataContracts;
using Expenses.Domain.Extensions;
using Expenses.Domain.Models;
using Libs.Api.Infra;
using Libs.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

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

    private BsonDocument BuildPaymentMethodAggregation() =>
        BuildAggregation(_dbContext.PaymentMethods.CollectionNamespace.CollectionName, nameof(Expense.PaymentMethodId), nameof(PaymentMethod));

    private BsonDocument BuildIdFilter(Guid id) =>
        BuildEqualFilter(nameof(Expense.Id), id);

    private BsonDocument BuildUserIdFilter(Guid id) =>
        BuildEqualFilter(nameof(Expense.UserId), id);

    public async Task<Expense> FindByIdAsync(Guid id, Guid userId)
    {
        var pipeline = new[] {
            BuildIdFilter(id),
            BuildEqualFilter(nameof(Expense.DeletedAt), null),
            BuildUserIdFilter(userId),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod))
        };

        return await Collection.Aggregate<Expense>(pipeline).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Expense entity)
    {
        var filter = IdFilter(entity.Id);
        await base.UpdateAsync(entity, filter);
    }

    public Task<long> GetAllCountAsync(ExpenseSearchParam searchParams, Guid userId)
    {
        var filter = _filterBuilder.And(_filterBuilder.Eq(x => x.UserId, userId), _filterBuilder.Eq(x => x.DeletedAt, null));
        filter = DefineFilters(searchParams, filter);
        filter = BuildDateFilter(filter, nameof(Expense.TransactionDate), searchParams.TransactionDate);
        return base.GetAllCountAsync(filter);
    }

    public async Task<List<TResponse>> GetAllPagedAsync<TResponse>(
        ExpenseSearchParam searchParam,
        PagedRequest request,
        Guid userId)
    {
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        var aggregatedBson = new BsonDocument[] {
            BuildEqualFilter(nameof(Expense.UserId), userId),
            BuildDateFilter(nameof(Expense.TransactionDate), searchParam.TransactionDate),
            BuildEqualFilter(nameof(Expense.DeletedAt), null),
            BuildFilters(searchParam),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod)),
            BuildSorting(request, ref sortDefinition)
        };

        var pagedResult = await base.GetAllPagedAsync<TResponse>(request, aggregatedBson);

        return pagedResult;
    }

    public async Task<List<TResponse>> GetAllGroupedPagedAsync<TResponse>(
        ExpenseSearchParam searchParam,
        PagedRequest request,
        Guid userId)
    {
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        var aggregatedBson = new List<BsonDocument> {
            BuildEqualFilter(nameof(Expense.UserId), userId),
            BuildDateFilter(nameof(Expense.TransactionDate), searchParam.TransactionDate),
            BuildEqualFilter(nameof(Expense.DeletedAt), null),
            BuildFilters(searchParam),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod)),
        };

        aggregatedBson.AddRange(BuildGrouping());

        var pagedResult = await Collection.Aggregate<TResponse>(aggregatedBson).ToListAsync();

        return pagedResult;
    }

    public async Task<List<string>> GetAllUserCategories(Guid userId)
    {
        var expenses = await Collection
            .Find(_filterBuilder.Eq(x => x.UserId, userId))
            .Project(e => e.ExpenseCategories)
            .ToListAsync();

        return expenses
            .SelectMany(e => e)
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    public List<BsonDocument> BuildGrouping()
    {
        return new List<BsonDocument> {
            new BsonDocument("$set", new BsonDocument
            {
                { 
                    "TransactionDateOnly", new BsonDocument
                    {
                        { "$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%d" },
                                { "date", "$TransactionDate" }
                            }
                        }
                    }
                }
            }),
            new BsonDocument("$group", new BsonDocument 
            {
                { "_id", "$TransactionDateOnly" },
                { "TotalPrice", new BsonDocument { { "$sum", "$TotalPrice" } } },
                { "Amount", new BsonDocument { { "$sum", 1 } } }
            }),
        };
    }
}