using Expenses.Api.PresentationContracts.Forms;
using Expenses.Api.PresentationContracts.PaymentMethods;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.DataContracts.Applications;

public interface IPaymentMethodApplication
{
    Task<PaymentMethodResponse> FetchByIdAsync(Guid id, Guid userId);
    Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId);
    Task UpdatePaymentMethodAsync(Guid id, Guid userId, JsonPatchDocument<PaymentMethodForm> patch);
    Task CreateNewPaymentMethodAsync(Guid userId, PaymentMethodForm form);
}

