using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Api.PresentationContracts.PaymentMethods;
using Transactions.Domain.Models;
using Transactions.Domain.Models.PaymentMethod;

namespace Transactions.Api.Adapters;

public class PaymentMethodAdapter : IPaymentMethodAdapter
{
    public PaymentMethod ConvertToDomain(PaymentMethodForm form) =>
        new(form.Name, form.PaymentType, form.IsDefault);

    public PaymentMethodForm ConvertToForm(PaymentMethod domain) =>
        new()
        {
            Name = domain.Name,
            PaymentType = domain.PaymentType,
            IsActive = domain.IsActive,
            IsDefault = domain.IsDefault
        };

    public PaymentMethodResponse ConvertToResponse(PaymentMethod domain) =>
        new()
        {
            Id = domain.Id,
            Name = domain.Name,
            PaymentType = domain.PaymentType.ToString(),
            IsActive = domain.IsActive,
            UserId = domain.UserId
        };

    public List<PaymentMethodResponse> ConvertToResponse(IEnumerable<PaymentMethod> domain) =>
        domain.Select(ConvertToResponse).ToList();
}
