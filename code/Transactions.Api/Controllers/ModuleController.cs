using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Forms;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/module")]
[Authorize]
public class ModuleController : ControllerBase
{
    private IModuleApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    public ModuleController(
        IModuleApplication moduleApplication,
        IOptions<CustomClaimSettings> claimSettings,
        ILogger<ModuleController> logger)
    {
        _application = moduleApplication;
        _claimSettings = claimSettings.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ModuleResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<List<ModuleResponse>>> FetchAllModulesByRoleAsync()
    {
        var result = await _application.FetchAllAsync(User);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateModule([FromBody] ModuleForm form)
    {
        await _application.CreateModuleAsync(form, UserId);
        return Accepted();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> DeleteModule(Guid id)
    {
        await _application.DeleteByIdAsync(id, UserId);
        return Accepted();
    }
}