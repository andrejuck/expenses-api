using System.Net;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.PresentationContracts.Forms;
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
    public async Task<ActionResult> CreateExpense([FromBody] ExpenseForm form)
    {
        await _application.CreateNewExpenseAsync(UserId, form);
        return Accepted();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateExpense(Guid id, [FromBody] JsonPatchDocument<ExpenseForm> patchForm)
    {
        await _application.UpdateExpenseAsync(id, UserId, patchForm);
        return Accepted();
    }
}