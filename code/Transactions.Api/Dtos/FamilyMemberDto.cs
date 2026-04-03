namespace Transactions.Api.Dtos;

public class FamilyMemberDto
{
    public required string MemberName { get; set; }
    public required Guid MemberId { get; set; }
}