using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.Adapters;

public class AccountAdapter : IAccountAdapter
{
    public Account ConvertToDomain(AccountForm form, Guid userId)
    {
        var account = new Account(form.AccountName, form.AccountType, form.InitialBalance);
        account.BindUser(userId);
        
        return account;
    }
    
    public AccountResponse ConvertToResponse(Account domain) =>
        new()
        {
            Id = domain.Id,
            Name = domain.Name,
            AccountType = domain.AccountType.ToString(),
            Balance = domain.Balance,
            FamilyId = domain.FamilyId,
            UserId = domain.UserId
        };

    public IEnumerable<AccountResponse> ConvertToResponse(IEnumerable<Account> entities) =>
        entities.Select(ConvertToResponse);
}