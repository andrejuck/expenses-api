using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models;
using Transactions.Domain.Models.PaymentMethod;

namespace Transactions.Domain.DataContracts;

public interface IPaymentMethodRepository : IBaseEntityRepository<PaymentMethod>
{
    Task<List<PaymentMethod>> FindAllByUserIdAsync(Guid userId);
    Task<PaymentMethod?> FindByNameAsync(string name, Guid userId);
}