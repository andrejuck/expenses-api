using Libs.Api.Infra;
using Libs.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Infra.Repositories;

public class AccountRepository(DBContext context)
    : BasePageableMongoRepository<Account>(context.Accounts), IAccountRepository
{
    public async Task<Account> UpdateAsync(Account entity)
    {
        var idFilter = _filterBuilder.Eq(account => account.Id, entity.Id);
        return await context.Accounts.FindOneAndReplaceAsync(idFilter, entity);
    }

    public async Task BindFamilyToAccountAsync(Guid familyId, Guid accountId)
    {
        var idFilter = _filterBuilder.Eq(account => account.Id, accountId);
        var updates = new List<UpdateDefinition<Account>>()
        {
            _updateBuilder.Set(x => x.FamilyId, familyId),
            _updateBuilder.Set(x => x.UpdatedAt, DateTime.UtcNow)
        };
        
        await context.Accounts.UpdateOneAsync(idFilter, _updateBuilder.Combine(updates));
    } 

    public async Task<Account?> FindByIdAsync(Guid id, Guid userId) =>
        await context.Accounts.Find(_filterBuilder.And(
            _filterBuilder.Eq(account => account.Id, id),
            _filterBuilder.Eq(account => account.UserId, userId)
        )).FirstOrDefaultAsync();

    public async Task<List<TResponse>> GetAllAccountsAsync<TResponse>(AccountSearchParam searchParams,
        PagedRequest pagedRequest, Guid userId)
    {
        var sortDefinition = _sortBuilder.Ascending(x => x.Name);
        var aggregationPipeline = new BsonDocument[]
        {
            BuildEqualFilter(nameof(Account.UserId), userId),
            BuildEqualFilter(nameof(Account.DeletedAt), null),
            BuildFilters(searchParams),
            BuildSorting(pagedRequest, ref sortDefinition),
            BuildFamilyAggregation(),
            BuildFlatChildAggregation(nameof(Family))
        };

        return await base.GetAllPagedAsync<TResponse>(pagedRequest, aggregationPipeline);
    }

    public async Task<IEnumerable<Account>> GetAllUserAccountsAsync(Guid userId) =>
        await context.Accounts.Find(_filterBuilder.And(
            _filterBuilder.Eq(acc => acc.UserId, userId)
            ))
            .ToListAsync();

    public async Task<long> GetAllCountAsync(AccountSearchParam searchParams, Guid userId)
    {
        var filter = _filterBuilder.And(
            _filterBuilder.Eq(x => x.UserId, userId),
            _filterBuilder.Eq(x => x.DeletedAt, null)
        );

        filter = DefineFilters(searchParams, filter);
        return await base.GetAllCountAsync(filter);
    }

    public async Task<Account?> FetchByIdAsync(Guid formAccountGuid) =>
        await context.Accounts.Find(
            _filterBuilder.Eq(account => account.Id, formAccountGuid)
        ).FirstOrDefaultAsync();

    public async Task<Account?> FetchByNameAsync(string formAccountName, Guid userId) =>
        await context.Accounts.Find(
            _filterBuilder.And(
                _filterBuilder.Eq(x => x.UserId, userId),
                _filterBuilder.Eq(x => x.Name, formAccountName)
            )
        ).FirstOrDefaultAsync();
    
    public async Task<Account?> FetchByNameAsync(string formAccountName, Guid accountId, Guid userId) =>
        await context.Accounts.Find(
            _filterBuilder.And(
                _filterBuilder.Eq(x => x.UserId, userId),
                _filterBuilder.Eq(x => x.Name, formAccountName),
                _filterBuilder.Ne(x => x.Id, accountId)
            )
        ).FirstOrDefaultAsync();

    public async Task<IEnumerable<Account>> FetchRecentAccountsSortedByUpdateDateAsync(Guid userId, int limit)
    {
        var filter = _filterBuilder.Eq(x => x.UserId, userId);

        return await Collection
            .Aggregate()
            .Match(filter)
            .Sort(_sortBuilder.Descending(x => x.UpdatedAt))
            .Limit(limit)
            .ToListAsync();
    }
        

    private BsonDocument BuildFamilyAggregation() =>
        BuildAggregation(context.Families.CollectionNamespace.CollectionName, nameof(Account.FamilyId), nameof(Family));
}