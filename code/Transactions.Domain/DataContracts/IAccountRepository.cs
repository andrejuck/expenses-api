using Libs.Api.Models;
using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Dtos;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Domain.DataContracts;

public interface IAccountRepository : IBaseEntityRepository<Account>
{
    Task<List<TResponse>> GetAllAccountsAsync<TResponse>(AccountSearchParam searchParams, PagedRequest pagedRequest, Guid userId);
    Task BindFamilyToAccountAsync(Guid familyId, Guid accountId);
    Task<IEnumerable<Account>> GetAllUserAccountsAsync(Guid userId);
    Task<Account?> FetchByIdAsync(Guid formAccountGuid);
    Task<Account?> FetchByNameAsync(string formAccountName, Guid userId);
    Task<Account?> FetchByNameAsync(string formAccountName, Guid accountId, Guid userId);
    Task<IEnumerable<Account>> FetchRecentAccountsSortedByUpdateDateAsync(Guid userId, int limit);
}