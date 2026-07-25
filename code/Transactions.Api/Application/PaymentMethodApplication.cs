using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Api.PresentationContracts.PaymentMethods;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.ErrorHandling;
using Microsoft.AspNetCore.JsonPatch;
using System.Net;
using System.Text.Json;
using Transactions.Domain.Models.PaymentMethod;

namespace Transactions.Api.Application;

public class PaymentMethodApplication : IPaymentMethodApplication
{
    private readonly IPaymentMethodRepository _paymentRepository;
    private readonly IPaymentMethodAdapter _adapter;
    private readonly IErrorService _errorService;
    private readonly ILogger<PaymentMethodApplication> _logger;

    public PaymentMethodApplication(
        IPaymentMethodRepository paymentRepository,
        IPaymentMethodAdapter adapter,
        IErrorService errorService,
        ILogger<PaymentMethodApplication> logger)
    {
        _paymentRepository = paymentRepository;
        _adapter = adapter;
        _errorService = errorService;
        _logger = logger;
    }

    public async Task<PaymentMethodResponse?> FetchByIdAsync(Guid id, Guid userId)
    {
        var payment = await FindPaymentBydIdAsync(id, userId);
        if (payment is null) return null;

        var response = _adapter.ConvertToResponse(payment);
        return response;
    }

    public async Task<List<PaymentMethodResponse>> FetchByUserAsync(Guid userId)
    {
        var payments = await _paymentRepository.FindAllByUserIdAsync(userId);
        var result = _adapter.ConvertToResponse(payments);
        _logger.LogInformation(Messages.LOG_GET_PAGED_MULTIPLE_MESSAGE, nameof(PaymentMethodResponse), payments.Count, userId, JsonSerializer.Serialize(result));
        return result;
    }

    public async Task UpdatePaymentMethodAsync(Guid id, Guid userId, JsonPatchDocument<PaymentMethodForm> patch)
    {
        var payment = await FindPaymentBydIdAsync(id, userId);
        if (payment is null) return;

        var paymentEntityForm = _adapter.ConvertToForm(payment);

        patch.ApplyTo(paymentEntityForm);
        payment.PrepareToUpdate(paymentEntityForm.Name, paymentEntityForm.PaymentType, paymentEntityForm.IsActive);

        await _paymentRepository.UpdateAsync(payment);
        _logger.LogInformation(Messages.LOG_UPDATED_MESSAGE, nameof(PaymentMethod), userId, JsonSerializer.Serialize(payment));
    }

    public async Task CreateNewPaymentMethodAsync(Guid userId, PaymentMethodForm form)
    {
        var entity = _adapter.ConvertToDomain(form);

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
        _logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(PaymentMethod), userId, JsonSerializer.Serialize(entity));
    }

    private async Task<PaymentMethod?> FindPaymentBydIdAsync(Guid id, Guid userId)
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

        _logger.LogInformation(Messages.LOG_GET_SINGLE_MESSAGE, nameof(PaymentMethod), userId, JsonSerializer.Serialize(payment));
        return payment;
    }
}
