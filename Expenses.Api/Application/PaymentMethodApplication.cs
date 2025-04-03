using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Api.PresentationContracts.PaymentMethods;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.ErrorHandling;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Bson;
using System.Net;

namespace Expenses.Api.Application;

public class PaymentMethodApplication : IPaymentMethodApplication
{
    private readonly IPaymentMethodRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;
    private readonly ILogger<PaymentMethodApplication> _logger;

    public PaymentMethodApplication(
        IPaymentMethodRepository paymentRepository,
        IMapper mapper,
        IErrorService errorService,
        ILogger<PaymentMethodApplication> logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _errorService = errorService;
        _logger = logger;
    }

    public async Task<PaymentMethodResponse> FetchByIdAsync(Guid id, Guid userId)
    {
        var payment = await FindPaymentBydIdAsync(id, userId);
        if (payment is null) return null;

        var response = _mapper.Map<PaymentMethodResponse>(payment);
        return response;
    }

    public async Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId)
    {
        var payments = await _paymentRepository.FindAllByUserIdAsync(userId);
        var result = _mapper.Map<List<PaymentMethodResponse>>(payments);
        _logger.LogInformation(Messages.LOG_GET_MULTIPLE_MESSAGE, nameof(PaymentMethodResponse), payments.Count, userId, result.ToJson());
        return result;
    }

    public async Task UpdatePaymentMethodAsync(Guid id, Guid userId, JsonPatchDocument<PaymentMethodForm> patch)
    {
        var payment = await FindPaymentBydIdAsync(id, userId);
        if (payment is null) return;

        var paymentEntityForm = _mapper.Map<PaymentMethodForm>(payment);

        patch.ApplyTo(paymentEntityForm);
        payment.PrepareToUpdate(paymentEntityForm.Name, paymentEntityForm.PaymentType, paymentEntityForm.IsActive);
        await _paymentRepository.UpdateAsync(payment);
        _logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(PaymentMethod), userId, payment.ToJson());
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

            return;
        }

        entity.BindUser(userId);
        await _paymentRepository.AddAsync(entity);
        _logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(PaymentMethod), userId, entity.ToJson());
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

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(PaymentMethod), userId, payment.ToJson());
        return payment;
    }
}