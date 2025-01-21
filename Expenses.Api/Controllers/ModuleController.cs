using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expenses.Api.Controllers;

[ApiController]
[Route("api/module")]
[Authorize]
public class ModuleController : ControllerBase
{

    public IModuleRepository _moduleRepository;
    private IMapper _mapper;

    public ModuleController(IModuleRepository moduleRepository, IMapper mapper)
    {
        _moduleRepository = moduleRepository;
        _mapper = mapper;
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<ModuleResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<List<ModuleResponse>>> FetchAllModulesByRoleAsync()
    {
        var userRoles = User.FindAll(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
        var modules = await _moduleRepository.FindAllByRolesAsync(userRoles);

        var result = _mapper.Map<List<ModuleResponse>>(modules);

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
        var entity = _mapper.Map<Module>(form);

        if (await _moduleRepository.FindByNameAsync(form.Name) is not null)
        {
            return Conflict(
                string.Format(
                    Messages.CONFLICT_MESSAGE_PATTERN,
                    nameof(Module),
                    nameof(ModuleForm.Name),
                    form.Name
                ));
        }

        await _moduleRepository.AddAsync(entity);

        return Accepted();
    }

    [HttpDelete]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> DeleteModule([FromQuery] Guid moduleId)
    {
        var existingModule = await _moduleRepository.GetByIdAsync(moduleId);
        if (existingModule is null)
        {
            return NotFound(
                string.Format(
                    Messages.NOT_FOUND_MESSAGE_PATTERN,
                    nameof(Module),
                    nameof(Module.Id),
                    moduleId
                ));
        }

        existingModule.SetDeleted();
        await _moduleRepository.UpdateAsync(existingModule);

        return Accepted();
    }
}