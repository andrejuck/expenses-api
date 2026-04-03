using Transactions.Api.Dtos;

namespace Transactions.Api.PresentationContracts.Families;

public class FamilyResponse
{
    public required Guid Id { get; set; }
    public required string FamilyName { get; set; }
    public required string OwnerName { get; set; }
    public required IEnumerable<FamilyMemberDto> Members { get; set; }
    public required IEnumerable<AccountDto> Accounts { get; set; }
    
}