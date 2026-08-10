using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.Infra;
using Libs.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Transactions.Domain.Models.PaymentMethod;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Infra.Repositories;

public class ExpenseRepository(DBContext dbContext)
    : BasePageableMongoRepository<Transaction>(dbContext.Expenses), IExpenseRepository
{
    private FilterDefinition<Transaction> IdFilter(Guid id)
    {
        return _filterBuilder.Eq(x => x.Id, id);
    }

    private BsonDocument BuildPaymentMethodAggregation() => BuildAggregation(dbContext.PaymentMethods.CollectionNamespace.CollectionName, nameof(Transaction.PaymentMethodId), nameof(PaymentMethod));

    private BsonDocument BuildIdFilter(Guid id) =>
        BuildEqualFilter(nameof(Transaction.Id), id);

    private BsonDocument BuildUserIdFilter(Guid id) =>
        BuildEqualFilter(nameof(Transaction.UserId), id);

    public async Task<Transaction?> FindByIdAsync(Guid id, Guid userId)
    { 
        var pipeline = new[] {
            BuildIdFilter(id),
            BuildEqualFilter(nameof(Transaction.DeletedAt), null),
            BuildUserIdFilter(userId),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod))
        };

        return await Collection.Aggregate<Transaction>(pipeline).FirstOrDefaultAsync();
    }

    public async Task<Transaction> UpdateAsync(Transaction entity)
    {
        var filter = IdFilter(entity.Id);
        await base.UpdateAsync(entity, filter);
        return entity;
    }

    public async Task<long> GetAllCountAsync(TransactionSearchParam searchParams, Guid userId)
    {
        var filter = _filterBuilder.And(_filterBuilder.Eq(x => x.UserId, userId), _filterBuilder.Eq(x => x.DeletedAt, null));
        filter = DefineFilters(searchParams, filter);
        filter = BuildDateFilter(filter, nameof(Transaction.TransactionDate), searchParams.StartTransactionDate, searchParams.EndTransactionDate);
        return await base.GetAllCountAsync(filter);
    }

    public async Task<List<TResponse>> GetAllPagedAsync<TResponse>(
        TransactionSearchParam searchParam,
        PagedRequest request,
        Guid userId)
    {
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        var aggregatedBson = new BsonDocument[] {
            BuildEqualFilter(nameof(Transaction.UserId), userId),
            BuildDateFilter(nameof(Transaction.TransactionDate), searchParam.StartTransactionDate, searchParam.EndTransactionDate),
            BuildEqualFilter(nameof(Transaction.DeletedAt), null),
            BuildFilters(searchParam),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod)),
            BuildSorting(request, ref sortDefinition)
        };

        var pagedResult = await base.GetAllPagedAsync<TResponse>(request, aggregatedBson);

        return pagedResult;
    }

    public async Task<List<TResponse>> GetAllGroupedPagedAsync<TResponse>(
        TransactionSearchParam searchParam,
        PagedRequest request,
        Guid userId)
    {
        //TODO - Apply pagination
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        var aggregatedBson = new List<BsonDocument> {
            BuildEqualFilter(nameof(Transaction.UserId), userId),
            BuildDateFilter(nameof(Transaction.TransactionDate), searchParam.StartTransactionDate, searchParam.EndTransactionDate),
            BuildEqualFilter(nameof(Transaction.DeletedAt), null),
            BuildFilters(searchParam),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod)),
        };

        aggregatedBson.AddRange(BuildGrouping());

        var pagedResult = await Collection.Aggregate<TResponse>(aggregatedBson).ToListAsync();

        return pagedResult;
    }

    public async Task<List<TResponse>> GetAllAsync<TResponse>(
        TransactionSearchParam searchParam,
        Guid userId)
    {
        var sortDefinition = _sortBuilder.Descending(x => x.TransactionDate);
        var aggregatedBson = new List<BsonDocument> {
            BuildEqualFilter(nameof(Transaction.UserId), userId),
            BuildDateFilter(nameof(Transaction.TransactionDate), searchParam.StartTransactionDate, searchParam.EndTransactionDate),
            BuildEqualFilter(nameof(Transaction.DeletedAt), null),
            BuildFilters(searchParam),
            BuildPaymentMethodAggregation(),
            BuildFlatChildAggregation(nameof(PaymentMethod)),
        };

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