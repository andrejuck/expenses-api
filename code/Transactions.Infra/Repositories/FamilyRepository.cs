using Libs.Api.Infra;
using MongoDB.Driver;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models.Families;

namespace Transactions.Infra.Repositories;

public class FamilyRepository(DBContext context)
    : BasePageableMongoRepository<Family>(context.Families), IFamilyRepository
{
    
    
    public Task<Family> UpdateAsync(Family entity)
    {
        throw new NotImplementedException();
    }

    public Task<Family?> FindByIdAsync(Guid id, Guid userId)
    {
        throw new NotImplementedException();
    }
}