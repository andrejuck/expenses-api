using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.DataContracts.Adapters;

public interface IAccountAdapter
{
    Account ConvertToDomain(AccountForm form, Guid userId);
    AccountResponse ConvertToResponse(Account domain);
    IEnumerable<AccountResponse> ConvertToResponse(IEnumerable<Account> entities);
    IEnumerable<RecentAccountResponse> ConvertToRecentAccountResponse(IEnumerable<Account> entities);
}