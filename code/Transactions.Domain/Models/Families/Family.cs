using Libs.Api.ErrorHandling.Exceptions;
using Libs.Auth.Models;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Models.Families;

public class Family : BaseEntity
{
    public Family(string name,
        Guid ownerUser,
        IEnumerable<FamilyMember> members,
        IEnumerable<Account> accounts)
    {
        Name = name;
        OwnerUser = ownerUser;
        Members = members;
        Accounts = accounts;
        
        Validate();
    }
    
    public string Name { get; set; } 
    public Guid OwnerUser { get; set; }
    public IEnumerable<FamilyMember> Members { get; set; }
    public IEnumerable<Account> Accounts { get; set; }

    public void PrepareToUpdate(string familyName, 
        IEnumerable<FamilyMember> members, 
        IEnumerable<Account> accounts)
    {
        Name = familyName;
        Members = members;
        Accounts = accounts;
        
        Validate();
    }

    private void Validate()
    {
        if (!Members.Any() || Members.Count() > 5)
            throw new DomainException("A family must have at least 1 member and a maximum of 5 members");
        if (!Accounts.Any())
            throw new DomainException("A family must have at least 1 shared account");
        if (Accounts.Any(account => account.AccountType.Equals(AccountType.Personal)))
            throw new DomainException("A family should not have personal accounts");
    }
}