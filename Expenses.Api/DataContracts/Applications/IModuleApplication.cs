using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using System.Security.Claims;

namespace Expenses.Api.DataContracts.Applications;

public interface IModuleApplication
{
    Task CreateModuleAsync(ModuleForm form, Guid userId);
    Task<List<ModuleResponse>> FetchAllAsync(ClaimsPrincipal user);
    Task DeleteByIdAsync(Guid moduleId, Guid userId);
}