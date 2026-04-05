using System.Security.Claims;
using Libs.Api.Models;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.DataContracts.Applications;

public interface IAccountApplication
{
    Task<AccountResponse> CreateAccountAsync(AccountForm form, Guid userId);
    Task<AccountResponse?> UpdateAccountAsync(Guid accountId, AccountForm form, Guid userId);
    Task BindFamilyToAccountAsync(Guid familyId, IEnumerable<Account> accounts);
    Task<PagedResponse<AccountResponse>> FetchPagedAccountsAsync(AccountSearchParam searchParams,
        PagedRequest pagedRequest, Guid userId);
    Task<AccountResponse?> FetchAccountByIdAsync(Guid id, Guid userId);
    Task DeleteByIdAsync(Guid id, Guid userId);
    Task<Account?> FetchAccountByIdAsync(Guid id);
}