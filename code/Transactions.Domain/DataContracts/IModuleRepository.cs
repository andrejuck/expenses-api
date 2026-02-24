using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models;

namespace Transactions.Domain.DataContracts;

public interface IModuleRepository : IBaseRepository<Module>
{
    Task<Module> FindByIdAsync(Guid id);
    Task<List<Module>> FindAllByRolesAsync(IEnumerable<string> roles);
    Task<Module> FindByNameAsync(string name);
}