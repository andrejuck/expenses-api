using Expenses.Domain.Models;
using Libs.Api.Infra;

namespace Expenses.Domain.DataContracts;

public interface IModuleRepository : IBaseRepository<Module>
{
    Task<List<Module>> FindAllByRolesAsync(IEnumerable<string> roles);
    Task<Module> FindByNameAsync(string name);
}