using System.Net;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController(
    IAccountApplication application,
    IOptions<CustomClaimSettings> claimSettings)
    : ControllerBase
{
    private readonly CustomClaimSettings _claimSettings = claimSettings.Value;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    [HttpGet("{id:guid}", Name = "FetchAccountById")]
    [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<AccountResponse>> FetchAccountByIdAsync(Guid id)
    {
        var result = await application.FetchAccountByIdAsync(id, UserId);
        return Ok(result);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<AccountResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<AccountResponse>>> FetchPagedUserAccountsAsync(
        [FromQuery] AccountSearchParam searchParams,
        [FromQuery] PagedRequest pagedRequest)
    {
        var result = await application.FetchPagedAccountsAsync(searchParams, pagedRequest, UserId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateAccount([FromBody] AccountForm form)
    {
        var response = await application.CreateAccountAsync(form, UserId);
        return CreatedAtRoute("FetchAccountById", new { id = response.Id }, response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> UpdateAccount(Guid id, [FromBody] AccountForm form)
    {
        var response = await application.UpdateAccountAsync(id, form, UserId);
        
        if(response is null)
            return Ok(response);

        return AcceptedAtRoute("FetchAccountById", new { id = response.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> DeleteModule(Guid id)
    {
        await application.DeleteByIdAsync(id, UserId);
        return NoContent();
    }
}