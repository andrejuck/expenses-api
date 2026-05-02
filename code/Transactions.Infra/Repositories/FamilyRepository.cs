using Libs.Api.Infra;
using Microsoft.CodeAnalysis.Operations;
using MongoDB.Bson;
using MongoDB.Driver;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models.Accounts;
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
        return await context.Families
            .Find(_filterBuilder.Eq(fam => fam.Id, id))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Family>> FetchAllUserFamiliesAsync(Guid userId)
    {
        return await context.Families
            .Find(
                _filterBuilder.Or(
                    _filterBuilder.ElemMatch(
                        family => family.Members,
                        member => member.Id.Equals(userId)
                    ),
                    _filterBuilder.Eq(family => family.OwnerUserId, userId)
                )
            ).ToListAsync();
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
}