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

    public async Task<Account?> FindByIdAsync(Guid id, Guid userId) =>
        await context.Accounts.Find(_filterBuilder.And(
            _filterBuilder.Eq(account => account.Id, id),
            _filterBuilder.Eq(account => account.UserId, userId)
        )).FirstOrDefaultAsync();

    public async Task<List<TResponse>> GetAllPagedAsync<TResponse>(AccountSearchParam searchParams, PagedRequest pagedRequest, Guid userId)
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

    public async Task<long> GetAllCountAsync(AccountSearchParam searchParams, Guid userId)
    {
        var filter = _filterBuilder.And(
            _filterBuilder.Eq(x => x.UserId, userId), 
            _filterBuilder.Eq(x => x.DeletedAt, null)
        );
        
        filter = DefineFilters(searchParams, filter);
        return await base.GetAllCountAsync(filter);
    }
    
    private BsonDocument BuildFamilyAggregation() =>
        BuildAggregation(context.Families.CollectionNamespace.CollectionName, nameof(Account.FamilyId), nameof(Family));
}