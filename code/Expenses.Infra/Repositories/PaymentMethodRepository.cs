using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.Infra;
using MongoDB.Driver;

namespace Expenses.Infra.Repositories;

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

    public async Task<PaymentMethod> FindByIdAsync(Guid id, Guid userId)
    {
        var filter = _filterBuilder.And(IdFilter(id), UserIdFilter(userId));

        return await _dbContext.PaymentMethods.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<PaymentMethod> FindByNameAsync(string name, Guid userId)
    {
        var filter = _filterBuilder.Eq(x => x.Name, name);

        return await _dbContext.PaymentMethods.Find(_filterBuilder.And(UserIdFilter(userId), filter)).FirstOrDefaultAsync();
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