using System.Security.Claims;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;

namespace Expenses.Api.DataContracts.Applications;

public interface IModuleApplication
{
    Task CreateModuleAsync(ModuleForm form);
    Task<List<ModuleResponse>> FetchAllAsync(ClaimsPrincipal user);
    Task DeleteByIdAsync(Guid moduleId);
}