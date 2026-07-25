using Transactions.Api.PresentationContracts.Forms;
using Transactions.Api.PresentationContracts.PaymentMethods;
using Microsoft.AspNetCore.JsonPatch;

namespace Transactions.Api.DataContracts.Applications;

public interface IPaymentMethodApplication
{
    Task<PaymentMethodResponse?> FetchByIdAsync(Guid id, Guid userId);
    Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId);
    Task UpdatePaymentMethodAsync(Guid id, Guid userId, JsonPatchDocument<PaymentMethodForm> patch);
    Task CreateNewPaymentMethodAsync(Guid userId, PaymentMethodForm form);
}

