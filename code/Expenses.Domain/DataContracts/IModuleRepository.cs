using Expenses.Domain.DataContracts.Generics;
using Expenses.Domain.Models;

namespace Expenses.Domain.DataContracts;

public interface IModuleRepository : IBaseRepository<Module>
{
    Task<Module> FindByIdAsync(Guid id);
    Task<List<Module>> FindAllByRolesAsync(IEnumerable<string> roles);
    Task<Module> FindByNameAsync(string name);
}