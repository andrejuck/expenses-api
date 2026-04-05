using Libs.Auth.Models;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.DataContracts.Adapters;

public interface IFamilyAdapter
{
    FamilyResponse ConvertToResponse(Family entity);
    List<FamilyResponse> ConvertToResponse(IEnumerable<Family> families);
    Family ConvertToDomain(FamilyForm form, Guid userId, string userName, IEnumerable<Account> accounts, IEnumerable<FamilyMember> members);
    IEnumerable<FamilyMember> ConvertToDomain(IEnumerable<string> emails);
    IEnumerable<FamilyAccount> ConvertToDomain(IEnumerable<Account> accounts);
    FamilyMember ConvertToDomain(User user);
    FamilyAccount ConvertToDomain(Account account);
}