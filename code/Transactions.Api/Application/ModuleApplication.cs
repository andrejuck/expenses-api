using AutoMapper;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.ErrorHandling;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Net;
using System.Security.Claims;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Application;

public class ModuleApplication : IModuleApplication
{
    private readonly IModuleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IErrorService _errorService;
    private readonly ILogger<ModuleApplication> _logger;
    private readonly CustomClaimSettings _customClaimSettings;

    public ModuleApplication(
        IModuleRepository repository,
        IMapper mapper,
        IErrorService errorService,
        ILogger<ModuleApplication> logger,
        IOptions<CustomClaimSettings> options)
    {
        _repository = repository;
        _mapper = mapper;
        _errorService = errorService;
        _logger = logger;
        _customClaimSettings = options.Value;
    }

    public async Task CreateModuleAsync(ModuleForm form, Guid userId)
    {
        var entity = _mapper.Map<Module>(form);
        entity.BindUser(userId);

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
        _logger.LogInformation(Messages.LOG_CREATED_MESSAGE, nameof(Transaction), userId, entity.ToJson());
    }

    public async Task<List<ModuleResponse>> FetchAllAsync(ClaimsPrincipal user)
    {
        var userRoles = user.FindAll(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
        var modules = await _repository.FindAllByRolesAsync(userRoles);

        var result = _mapper.Map<List<ModuleResponse>>(modules);
        _logger.LogInformation(
            Messages.LOG_GET_MULTIPLE_MESSAGE,
            result.Count,
            nameof(ModuleResponse),
            result.Count,
            UserClaimsHelper.GetUserGuidIdFromClaims(user, _customClaimSettings)
        );

        return result;
    }

    public async Task DeleteByIdAsync(Guid moduleId, Guid userId)
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
        _logger.LogInformation(Messages.LOG_DELETED_MESSAGE, nameof(existingModule), userId, existingModule.ToJson());
    }
}