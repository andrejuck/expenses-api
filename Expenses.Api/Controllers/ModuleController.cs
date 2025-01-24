using System.Net;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expenses.Api.Controllers;

[ApiController]
[Route("api/module")]
[Authorize]
public class ModuleController : ControllerBase
{
    private IModuleApplication _application;

    public ModuleController(IModuleApplication moduleApplication)
    {
        _application = moduleApplication;
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
        await _application.CreateModuleAsync(form);
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
        await _application.DeleteByIdAsync(id);
        return Accepted();
    }
}