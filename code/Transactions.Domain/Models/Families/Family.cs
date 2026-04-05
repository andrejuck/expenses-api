using Libs.Api.ErrorHandling.Exceptions;
using Transactions.Domain.Exceptions;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Models.Families;

public class Family : BaseEntity
{
    public Family(string name,
        Guid ownerUser,
        string ownerUserName,
        IEnumerable<FamilyMember> members,
        IEnumerable<FamilyAccount> accounts)
    {
        Name = name;
        OwnerUserId = ownerUser;
        OwnerUserName = ownerUserName;
        Members = members;
        Accounts = accounts;
        
        Validate();
    }
    
    public string Name { get; set; } 
    public Guid OwnerUserId { get; set; }
    public string OwnerUserName { get; set; }
    public IEnumerable<FamilyMember> Members { get; set; }
    public IEnumerable<FamilyAccount> Accounts { get; set; }

    public void PrepareToUpdate(string familyName, 
        IEnumerable<FamilyMember> members, 
        IEnumerable<FamilyAccount> accounts)
    {
        Name = familyName;
        Members = members;
        Accounts = accounts;
        
        Validate();
    }

    private void Validate()
    {
        if (!Members.Any() || Members.Count() > 5)
            throw new DomainException(DomainMessages.FAMILY_INVALID_MEMBERS_QUANTITY);
        if (!Accounts.Any())
            throw new DomainException(DomainMessages.FAMILY_INVALID_ACCOUNTS_QUANTITY);
        if (Accounts.Any(account => account.AccountType.Equals(AccountType.Personal)))
            throw new DomainException(DomainMessages.FAMILY_INVALID_ACCOUNTS_TYPE);
    }
}