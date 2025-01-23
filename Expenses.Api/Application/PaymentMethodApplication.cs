using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.PresentationContracts;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;

namespace Expenses.Api.Application;

public class PaymentMethodApplication : IPaymentMethodApplication
{
    private readonly IPaymentMethodRepository _paymentRepository;
    private readonly IMapper _mapper;

    public PaymentMethodApplication(IPaymentMethodRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<PaymentMethod> FetchByIdAsync(Guid id, Guid userId)
    {
        return await _paymentRepository.FindByIdAsync(id, userId);
        //TODO - Implement error treatments
    }

    public async Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId)
    {
        var payments = await _paymentRepository.FindAllByUserIdAsync(userId);
        var result = _mapper.Map<List<PaymentMethodResponse>>(payments);

        return result;
    }
}