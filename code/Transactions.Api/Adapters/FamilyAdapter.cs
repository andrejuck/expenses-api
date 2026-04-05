using Libs.Auth.Models;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.Dtos;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.Adapters;

public class FamilyAdapter : IFamilyAdapter
{
    public FamilyResponse ConvertToResponse(Family entity) =>
        new()
        {
            Id = entity.Id,
            FamilyName = entity.Name,
            OwnerName = entity.OwnerUserName,
            Members = ConvertToDto(entity.Members),
            Accounts = ConvertToDto(entity.Accounts),
        };

    public List<FamilyResponse> ConvertToResponse(IEnumerable<Family> families) =>
        families.Select(ConvertToResponse).ToList();

    public Family ConvertToDomain(FamilyForm form, 
        Guid userId, 
        string userName,
        IEnumerable<Account> accounts,
        IEnumerable<FamilyMember> members) =>
        new(form.FamilyName,
            userId,
            userName,
            members,
            ConvertToDomain(accounts));

    public IEnumerable<FamilyMember> ConvertToDomain(IEnumerable<string> emails)
    {
        return emails.Select(email => new FamilyMember()
        {
            Email = email,
            Id = Guid.Parse("f526bcd2-75f2-436d-8605-12440771340d"),
            Username = email,
        });
    }
    
    public IEnumerable<FamilyAccount> ConvertToDomain(IEnumerable<Account> accounts) =>
        accounts.Select(account => new FamilyAccount
        {
            Id = account.Id,
            AccountName = account.Name,
            AccountType = account.AccountType
        });

    public FamilyMember ConvertToDomain(User user) =>
        new()
        {
            Email = user.Email,
            Id = user.Id,
            Username = user.Username,
        };

    public FamilyAccount ConvertToDomain(Account account) =>
        new()
        {
            Id = account.Id,
            AccountName = account.Name,
            AccountType = account.AccountType
        };

    private IEnumerable<FamilyMemberDto> ConvertToDto(IEnumerable<FamilyMember> entityMembers) =>
        entityMembers.Select(familyMember =>
            new FamilyMemberDto
            {
                MemberName = familyMember.Username,
                MemberId = familyMember.Id,
                MemberEmail = familyMember.Email
            });

    private IEnumerable<AccountDto> ConvertToDto(IEnumerable<FamilyAccount> entityAccounts) =>
        entityAccounts.Select(account =>
            new AccountDto
            {
                AccountId = account.Id,
                AccountName = account.AccountName,
            });
}