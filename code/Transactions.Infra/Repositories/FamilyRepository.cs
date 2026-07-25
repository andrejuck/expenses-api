using Libs.Api.Infra;
using Libs.Auth.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Dtos;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Families;

namespace Transactions.Infra.Repositories;

public class FamilyRepository(DBContext context)
    : BasePageableMongoRepository<Family>(context.Families), IFamilyRepository
{
    
    //TODO - Add to Libs
    public async Task<Family> UpdateAsync(Family entity)
    {
        var idFilter = _filterBuilder.Eq(family => family.Id, entity.Id);
        return await context.Families.FindOneAndReplaceAsync(idFilter, entity);
    }

    public async Task<Family?> FindByIdAsync(Guid id, Guid userId)
    {
        var idFilter = _filterBuilder.And(
            _filterBuilder.Eq(fam => fam.Id, id)
            );
        
        return await context.Families
            .Find(idFilter)
            .FirstOrDefaultAsync();
    }
    
    public async Task<FamilyDto?> FindAndProjectByIdAsync(Guid id)
    {
        var idFilter = _filterBuilder.Eq(fam => fam.Id, id);
        return await BuildLookup(idFilter)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FamilyDto>> FetchAllUserFamiliesAsync(Guid userId)
    {
        var filter = _filterBuilder.Or(
            _filterBuilder.ElemMatch(
                family => family.Members,
                member => member == userId
            ),
            _filterBuilder.Eq(family => family.OwnerUserId, userId)
        );
        
        return await BuildLookup(filter)
            .Sort(Builders<FamilyDto>.Sort.Ascending(x => x.FamilyName))
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Account>> GetAllUserFamilyAccountsAsync(Guid userId)
    {
        var aggregationPipeline = new []
        {
            BuildElemMatch("_id", userId),
            BuildAggregation(context.Accounts.CollectionNamespace.CollectionName, "Accounts._id", nameof(Account)),
            BuildUnwind(nameof(Account)),
            BuildReplaceRoot(nameof(Account))
        };
        
        var result = await Collection.Aggregate<Account>(aggregationPipeline).ToListAsync(); 
        return result;
    }

    private BsonDocument BuildElemMatch(string propName, Guid propValue)
    {
        return new BsonDocument("$match", new BsonDocument
        {
            {
                "Members", new BsonDocument
                {
                    {
                        "$elemMatch", new BsonDocument
                        {
                            { propName, new BsonBinaryData(propValue, GuidRepresentation.Standard) }
                        }
                    }
                }
            }
        });
    }

    private BsonDocument BuildUnwind(string fieldName)
    {
        return new BsonDocument("$unwind", "$" + fieldName);
    }

    private BsonDocument BuildReplaceRoot(string fieldName)
    {
        return new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$" + fieldName));
    }

    private IAggregateFluent<FamilyDto> BuildLookup(FilterDefinition<Family> filter)
    {
        return context.Families
            .Aggregate()
            .Match(filter)
            .Lookup<Family, Account, FamilyDto>(context.Accounts,
                fam => fam.Accounts,
                account => account.Id,
                result => result.Accounts)
            .Lookup<FamilyDto, User, FamilyDto>(context.Users,
                fam => fam.Members,
                user => user.Id,
                result => result.Members);
    }
}