using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models;

namespace Transactions.Api.DataContracts.Adapters;

public interface IModuleAdapter
{
    Module ConvertToDomain(ModuleForm form, Guid userId);
    List<ModuleResponse> ConvertToResponse(IEnumerable<Module> domain);
}
