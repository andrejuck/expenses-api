using System.Net;
using System.Security.Claims;
using AutoMapper;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Helpers;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.DataContracts;
using Expenses.Domain.Models;
using Libs.Api.ErrorHandling;

namespace Expenses.Api.Application;

public class ModuleApplication : IModuleApplication
{
    private readonly IModuleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;

    public ModuleApplication(
        IModuleRepository repository,
        IMapper mapper,
        IErrorService errorService)
    {
        _repository = repository;
        _mapper = mapper;
        _errorService = errorService;
    }

    public async Task CreateModuleAsync(ModuleForm form)
    {
        var entity = _mapper.Map<Module>(form);

        if (await _repository.FindByNameAsync(form.Name) is not null)
        {
            _errorService.AddError(
                nameof(CreateModuleAsync),
                string.Format(
                    Messages.CONFLICT_MESSAGE_PATTERN,
                    nameof(Module),
                    nameof(ModuleForm.Name),
                    form.Name
                ),
                HttpStatusCode.Conflict);

            return;
        }

        await _repository.AddAsync(entity);
    }

    public async Task<List<ModuleResponse>> FetchAllAsync(ClaimsPrincipal user)
    {
        var userRoles = user.FindAll(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
        var modules = await _repository.FindAllByRolesAsync(userRoles);

        var result = _mapper.Map<List<ModuleResponse>>(modules);
        return result;
    }

    public async Task DeleteByIdAsync(Guid moduleId)
    {
        var existingModule = await _repository.FindByIdAsync(moduleId);
        if (existingModule is null)
        {
            _errorService.AddError(
                nameof(DeleteByIdAsync),
                string.Format(
                    Messages.NOT_FOUND_MESSAGE_PATTERN,
                    nameof(Module),
                    nameof(Module.Id),
                    moduleId
                ),
                HttpStatusCode.NotFound);

            return;
        }

        existingModule.SetDeleted();
        await _repository.UpdateAsync(existingModule);
    }
}