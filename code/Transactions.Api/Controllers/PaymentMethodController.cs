using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Api.PresentationContracts.PaymentMethods;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/payment-method")]
[Authorize]
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    public PaymentMethodController(
        IOptions<CustomClaimSettings> claimSettings,
        IPaymentMethodApplication application)
    {
        _application = application;
        _claimSettings = claimSettings.Value;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreatePaymentMethod([FromBody] PaymentMethodForm form)
    {
        await _application.CreateNewPaymentMethodAsync(UserId, form);
        return Accepted();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> UpdatePaymentMethod(
        Guid id,
        [FromBody] JsonPatchDocument<PaymentMethodForm> partialEntity
        )
    {
        await _application.UpdatePaymentMethodAsync(id, UserId, partialEntity);
        return Accepted();
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> FetchByIdAsync(Guid id)
    {
        var response = await _application.FetchByIdAsync(id, UserId);
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