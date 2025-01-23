using Expenses.Api.PresentationContracts;
using Expenses.Domain.Models;

namespace Expenses.Api.DataContracts.Applications;

public interface IPaymentMethodApplication
{
    Task<PaymentMethod> FetchByIdAsync(Guid id, Guid userId);
    Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId);
}

