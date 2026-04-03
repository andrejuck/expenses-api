using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.DataContracts.Adapters;

public interface IAccountAdapter
{
    Account ConvertToDomain(AccountForm form, Guid userId);
    AccountResponse ConvertToResponse(Account domain);
}