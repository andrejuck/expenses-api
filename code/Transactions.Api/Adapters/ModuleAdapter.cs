using Libs.Auth.Models;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models;

namespace Transactions.Api.Adapters;

public class ModuleAdapter : IModuleAdapter
{
    public Module ConvertToDomain(ModuleForm form, Guid userId)
    {
        var allowedRoles = form.AllowedRoles
            .Select(role => Enum.Parse<UserRole>(role, ignoreCase: true))
            .ToArray();

        return new Module(form.Name, userId, allowedRoles);
    }

    public List<ModuleResponse> ConvertToResponse(IEnumerable<Module> domain) =>
        domain.Select(module => new ModuleResponse { Id = module.Id, Name = module.Name }).ToList();
}
