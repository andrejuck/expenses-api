using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/transaction")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);
    public TransactionController(ITransactionApplication application, IOptions<CustomClaimSettings> claimSettings)
    {
        _application = application;
        _claimSettings = claimSettings.Value;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateTransactionAsync([FromBody] TransactionForm form)
    {
        await _application.CreateNewTransactionAsync(UserId, form);
        return Accepted();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> PatchTransactionAsync(Guid id, [FromBody] JsonPatchDocument<TransactionForm> patchForm)
    {
        await _application.UpdateTransactionAsync(id, UserId, patchForm);
        return Accepted();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TransactionResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<TransactionResponse>>> GetPagedTransactionsAsync(
        [FromQuery] TransactionSearchParam searchParams,
        [FromQuery] PagedRequest pagedRequest)
    {
        var result = await _application.GetPagedTransactionAsync(searchParams, pagedRequest, UserId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<TransactionResponse>>> GetTransactionByIdAsync(Guid id)
    {
        var result = await _application.GetTransactionAsync(id, UserId);
        return Ok(result);
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<List<string>>> GetCategoriesAsync()
    {
        List<string> result = await _application.GetUserCategoriesAsync(UserId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<List<string>>> DeleteTransactionAsync(Guid id)
    {
        await _application.DeleteTransactionAsync(id, UserId);
        return Accepted();
    }
}