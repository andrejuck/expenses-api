using System.Net;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.Models;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Expenses.Api.Controllers;

[ApiController]
[Route("api/expense")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);
    public ExpenseController(IExpenseApplication application, IOptions<CustomClaimSettings> claimSettings)
    {
        _application = application;
        _claimSettings = claimSettings.Value;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateExpenseAsync([FromBody] ExpenseForm form)
    {
        await _application.CreateNewExpenseAsync(UserId, form);
        return Accepted();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateExpenseAsync(Guid id, [FromBody] JsonPatchDocument<ExpenseForm> patchForm)
    {
        await _application.UpdateExpenseAsync(id, UserId, patchForm);
        return Accepted();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ExpenseResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<ExpenseResponse>>> GetPagedExpenseAsync([FromQuery] ExpenseSearchParam searchParams, [FromQuery] PagedRequest pagedRequest)
    {
        var result = await _application.GetPagedExpenseAsync(searchParams, pagedRequest, UserId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExpenseResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<ExpenseResponse>>> GetExpenseByIdAsync(Guid id)
    {
        var result = await _application.GetExpenseAsync(id, UserId);
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
    public async Task<ActionResult<List<string>>> DeleteExpenseAsync(Guid id)
    {
        await _application.DeleteExpenseAsync(id, UserId);
        return Accepted();
    }
}