using Libs.Auth.Models;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.Dtos;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Dtos;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.Adapters;

public class FamilyAdapter : IFamilyAdapter
{
    public FamilyResponse ConvertToResponse(FamilyDto dto) =>
        new(dto.Id, 
            dto.FamilyName,
            dto.OwnerUserName,
            ConvertToResponse(dto.Members),
            ConvertToResponse(dto.Accounts)
        );

    private IEnumerable<FamilyMemberResponse> ConvertToResponse(IEnumerable<UserDto> dtoMembers) =>
        dtoMembers.Select(user => new FamilyMemberResponse(user.Username, user.Email));

    private IEnumerable<FamilyAccountResponse> ConvertToResponse(IEnumerable<AccountDto> dtoAccounts) =>
        dtoAccounts.Select(account => new FamilyAccountResponse(account.Id, account.AccountName, account.AccountType));

    public List<FamilyResponse> ConvertToResponse(IEnumerable<FamilyDto> dtoList) =>
        dtoList.Select(ConvertToResponse).ToList();

    public Family ConvertToDomain(FamilyForm form, 
        Guid userId, 
        string userName,
        IEnumerable<Guid> accounts,
        IEnumerable<Guid> members) =>
        new(form.FamilyName,
            userId,
            userName,
            members,
            accounts);
}