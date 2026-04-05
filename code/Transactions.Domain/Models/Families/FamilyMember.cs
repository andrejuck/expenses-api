namespace Transactions.Domain.Models.Families;

public class FamilyMember
{
    public required string Email { get; set; }
    public string? Username { get; set; }
    public Guid? Id { get; set; }
}