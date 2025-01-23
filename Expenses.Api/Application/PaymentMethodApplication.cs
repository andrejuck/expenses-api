using System.Net;
using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.ErrorHandling;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.Application;

public class PaymentMethodApplication : IPaymentMethodApplication
{
    private readonly IPaymentMethodRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;

    public PaymentMethodApplication(IPaymentMethodRepository paymentRepository, IMapper mapper, IErrorService errorService)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _errorService = errorService;
    }

    public async Task<PaymentMethodResponse> FetchByIdAsync(Guid id, Guid userId)
    {
        var payment = await FindPaymentBydIdAsync(id, userId);
        if(payment is null) return null;

        var response = _mapper.Map<PaymentMethodResponse>(payment);

        return response;
    }

    public async Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId)
    {
        var payments = await _paymentRepository.FindAllByUserIdAsync(userId);
        var result = _mapper.Map<List<PaymentMethodResponse>>(payments);

        return result;
    }

    public async Task UpdatePaymentMethodAsync(Guid id, Guid userId, JsonPatchDocument<PaymentMethodForm> patch)
    {
        var payment = await FindPaymentBydIdAsync(id, userId);
        if(payment is null) return;

        var paymentEntityForm = _mapper.Map<PaymentMethodForm>(payment);

        patch.ApplyTo(paymentEntityForm);
        payment.PrepareToUpdate(paymentEntityForm.Name, paymentEntityForm.PaymentType, paymentEntityForm.IsActive);
        await _paymentRepository.UpdateAsync(payment);
    }

    public async Task CreateNewPaymentMethodAsync(Guid userId, PaymentMethodForm form) 
    {
         var entity = _mapper.Map<PaymentMethod>(form);

        if (await _paymentRepository.FindByNameAsync(form.Name, userId) is not null)
        {
            _errorService.AddError(
                nameof(CreateNewPaymentMethodAsync),
                string.Format(
                    Messages.CONFLICT_MESSAGE_PATTERN,
                    nameof(PaymentMethod),
                    nameof(PaymentMethodForm.Name),
                    form.Name
                ),
                HttpStatusCode.Conflict);
        }

        entity.BindUser(userId);
        await _paymentRepository.AddAsync(entity);
    }

    private async Task<PaymentMethod> FindPaymentBydIdAsync(Guid id, Guid userId)
    {
        var payment = await _paymentRepository.FindByIdAsync(id, userId);

        if (payment is null)
        {
            _errorService.AddError(
                nameof(UpdatePaymentMethodAsync),
                string.Format(
                    Messages.NOT_FOUND_MESSAGE_PATTERN,
                    nameof(PaymentMethod),
                    nameof(PaymentMethod.Id),
                    id
                ),
                HttpStatusCode.NotFound);

            return null;
        }

        return payment;
    }
}