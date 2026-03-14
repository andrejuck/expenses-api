using AutoMapper;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Api.PresentationContracts.PaymentMethods;
using Transactions.Domain.Models;

namespace Transactions.Api.Adapters;

public class PaymentMethodProfile : Profile
{

    public PaymentMethodProfile()
    {
        CreateMap<PaymentMethodForm, PaymentMethod>();
        CreateMap<PaymentMethod, PaymentMethodForm>();
        CreateMap<PaymentMethod, PaymentMethodResponse>();
        CreateMap<PaymentMethodResponse, PaymentMethod>();
    }
}