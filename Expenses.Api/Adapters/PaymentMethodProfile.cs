using AutoMapper;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;
using Libs.Auth.Models;

namespace Expenses.Api.Adapters;

public class PaymentMethodProfile : Profile {

    public PaymentMethodProfile()
    {
        CreateMap<PaymentMethodForm, PaymentMethod>();
        CreateMap<PaymentMethod, PaymentMethodForm>();
        CreateMap<PaymentMethod, PaymentMethodResponse>();
    }
}