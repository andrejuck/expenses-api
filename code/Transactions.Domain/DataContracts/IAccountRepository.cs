using Libs.Api.Models;
using Transactions.Domain.DataContracts.Generics;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Domain.DataContracts;

public interface IAccountRepository : IBaseEntityRepository<Account>
{
    Task<List<TResponse>> GetAllAccountsAsync<TResponse>(AccountSearchParam searchParams, PagedRequest pagedRequest, Guid userId);
    Task<IEnumerable<Account>> GetAllUserAccountsAsync(Guid userId);
    Task<long> GetAllCountAsync(AccountSearchParam searchParams, Guid userId);
    Task<Account?> FetchByIdAsync(Guid formAccountGuid);
}