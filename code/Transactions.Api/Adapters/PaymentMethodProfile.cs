using AutoMapper;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Api.PresentationContracts.PaymentMethods;
using Transactions.Domain.Models;

namespace Expenses.Api.Adapters;

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