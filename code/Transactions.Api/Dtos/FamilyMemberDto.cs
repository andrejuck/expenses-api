namespace Transactions.Api.Dtos;

public class FamilyMemberDto
{
    public string? MemberName { get; set; }
    public Guid? MemberId { get; set; }
    public required string MemberEmail { get; set; }
}