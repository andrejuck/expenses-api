using Expenses.Domain.DataContracts.Generics;
using Expenses.Domain.Models;

namespace Expenses.Domain.DataContracts;

public interface IPaymentMethodRepository : IBaseEntityRepository<PaymentMethod>
{
    Task<List<PaymentMethod>> FindAllByUserIdAsync(Guid userId);
    Task<PaymentMethod> FindByNameAsync(string name, Guid userId);
}