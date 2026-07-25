using Transactions.Api.PresentationContracts.Forms;
using Transactions.Api.PresentationContracts.PaymentMethods;
using Transactions.Domain.Models;
using Transactions.Domain.Models.PaymentMethod;

namespace Transactions.Api.DataContracts.Adapters;

public interface IPaymentMethodAdapter
{
    PaymentMethod ConvertToDomain(PaymentMethodForm form);
    PaymentMethodForm ConvertToForm(PaymentMethod domain);
    PaymentMethodResponse ConvertToResponse(PaymentMethod domain);
    List<PaymentMethodResponse> ConvertToResponse(IEnumerable<PaymentMethod> domain);
}
