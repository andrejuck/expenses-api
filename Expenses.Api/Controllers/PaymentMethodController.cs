using System.Net;
using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Expenses.Api.Controllers;

[ApiController]
[Route("api/payment-method")]
[Authorize]
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPaymentMethodApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    public PaymentMethodController(IPaymentMethodRepository repository,
        IMapper mapper,
        IOptions<CustomClaimSettings> claimSettings,
        IPaymentMethodApplication application)
    {
        _repository = repository;
        _mapper = mapper;
        _application = application;
        _claimSettings = claimSettings.Value;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreatePaymentMethod([FromBody] PaymentMethodForm form)
    {
        var entity = _mapper.Map<PaymentMethod>(form);

        if (await _repository.FindByNameAsync(form.Name, UserId) is not null)
        {
            return Conflict(
                string.Format(
                    Messages.CONFLICT_MESSAGE_PATTERN,
                    nameof(PaymentMethod),
                    nameof(PaymentMethodForm.Name),
                    form.Name
                ));
        }

        entity.BindUser(UserId);
        await _repository.AddAsync(entity);

        return Accepted();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> InactivatePaymentMethod(
        Guid id,
        [FromBody] JsonPatchDocument<PaymentMethodForm> partialEntity
        )
    {

        var payment = await _repository.FindByIdAsync(id, UserId);

        if (payment is null)
        {
            return NotFound(
                string.Format(
                    Messages.NOT_FOUND_MESSAGE_PATTERN,
                    nameof(PaymentMethod),
                    nameof(PaymentMethod.Id),
                    id
                ));
        }

        var paymentEntityForm = _mapper.Map<PaymentMethodForm>(payment);

        partialEntity.ApplyTo(paymentEntityForm);
        payment.PrepareToUpdate(paymentEntityForm.Name, paymentEntityForm.PaymentType, paymentEntityForm.IsActive);
        await _repository.UpdateAsync(payment);

        return Accepted();
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> FetchByIdAsync(Guid id)
    {
        var payment = await _application.FetchByIdAsync(id, UserId);
        if (payment is null)
        {
            return NotFound(
                string.Format(
                    Messages.NOT_FOUND_MESSAGE_PATTERN,
                    nameof(PaymentMethod),
                    nameof(PaymentMethod.Id),
                    id
                ));
        }

        var response = _mapper.Map<PaymentMethodResponse>(payment);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> FetchAllByUserAsync()
    {
        return Ok(await _application.FetchByUserAsync(UserId));
    }
}