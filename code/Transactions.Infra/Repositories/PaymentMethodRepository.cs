using Transactions.Domain.DataContracts;
using Libs.Api.Infra;
using MongoDB.Driver;
using Transactions.Domain.Models.PaymentMethod;

namespace Transactions.Infra.Repositories;

public class PaymentMethodRepository : BaseMongoRepository<PaymentMethod>, IPaymentMethodRepository
{
    private readonly DBContext _dbContext;

    public PaymentMethodRepository(DBContext dbContext)
        : base(dbContext.PaymentMethods)
    {
        _dbContext = dbContext;
    }

    private FilterDefinition<PaymentMethod> IdFilter(Guid id)
    {
        return _filterBuilder.Eq(a => a.Id, id);
    }

    private FilterDefinition<PaymentMethod> UserIdFilter(Guid userId)
    {
        return _filterBuilder.Eq(a => a.UserId, userId);
    }

    public async Task<PaymentMethod?> FindByIdAsync(Guid id, Guid userId)
    {
        var filter = _filterBuilder.And(IdFilter(id), UserIdFilter(userId));

        return await _dbContext.PaymentMethods.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<PaymentMethod?> FindByNameAsync(string name, Guid userId, Guid? currentPaymentMethodId = null)
    {
        var filter = _filterBuilder.And(UserIdFilter(userId), _filterBuilder.Eq(x => x.Name, name));
        if (currentPaymentMethodId.HasValue)
        {
            filter = _filterBuilder.And(
                filter,
                _filterBuilder.Ne(x => x.Id, currentPaymentMethodId.Value));
        }

        return await _dbContext.PaymentMethods.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsDefaultAsync(Guid userId, Guid? currentPaymentMethodId)
    {
        var filter = UserIdFilter(userId);
        var defaultFilter = _filterBuilder.And(filter, _filterBuilder.Eq(x => x.IsDefault, true));
        if (currentPaymentMethodId.HasValue)
        {
            filter = _filterBuilder.And(
                defaultFilter,
                _filterBuilder.Ne(x => x.Id, currentPaymentMethodId.Value));
        }
        
        return await _dbContext.PaymentMethods.Find(filter).AnyAsync();
    }

    public async Task<PaymentMethod> UpdateAsync(PaymentMethod entity)
    {
        var filter = IdFilter(entity.Id);
        await base.UpdateAsync(entity, filter);
        return entity;
    }

    public async Task<List<PaymentMethod>> FindAllByUserIdAsync(Guid userId)
    {
        var filter = UserIdFilter(userId);

        return await _dbContext.PaymentMethods.Find(filter).ToListAsync();
    }
}