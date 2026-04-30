using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Domain.DataContracts;

public interface IFamilyRepository : IBaseEntityRepository<Family>
{
    Task<IEnumerable<Family>> FetchAllUserFamiliesAsync(Guid userId);
    Task<IEnumerable<Account>> GetAllUserFamilyAccountsAsync(Guid userId);
    
}