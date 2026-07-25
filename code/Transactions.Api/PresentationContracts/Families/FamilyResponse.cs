using MongoDB.Driver;
using Transactions.Api.Dtos;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Families;

namespace Transactions.Api.PresentationContracts.Families;

public record FamilyResponse(
    Guid Id,
    string FamilyName,
    string OwnerName,
    IEnumerable<FamilyMemberResponse> Members,
    IEnumerable<FamilyAccountResponse> Accounts);

public record FamilyAccountResponse(Guid Id, string AccountName, AccountType AccountType);
public record FamilyMemberResponse(string MemberName, string? MemberEmail);