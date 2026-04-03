using CsvHelper;
using Libs.Api.ErrorHandling.Exceptions;
using Transactions.Domain.Exceptions;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Families;

namespace Transactions.Domain.Models.Accounts;

public class Account : BaseUserEntity
{
     public Account(string name, 
          AccountType accountType,
          decimal? balance)
     {
          Name = name;
          AccountType = accountType;
          Balance = balance ?? 0;
          
          Validate();
     }
     public string Name { get; private set; }
     public AccountType AccountType { get; private set; }
     public Guid? FamilyId { get; set; }
     public decimal Balance { get; set; } = 0;

     public void PrepareToUpdate(string name,
          AccountType accountType,
          Guid? familyTypeId)
     {
          Name = name;
          AccountType = accountType;
          FamilyId = familyTypeId;
          
          SetUpdatedAt();
          Validate();
     }

     public void BindFamily(Family family)
     {
          FamilyId = family.Id;
     }

     private void Validate()
     {
          if(AccountType.Equals(AccountType.Personal) && FamilyId.HasValue)
               throw new DomainException(DomainMessages.ACCOUNT_FAMILYBIND_NOT_ALLOWED_FOR_PERSONAL_ACCOUNT);
     }
}