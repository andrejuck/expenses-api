namespace Transactions.Api.PresentationContracts;

public class AccountResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string AccountType { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0;
    public Guid? FamilyId { get; set; }
    public string? FamilyName { get; set; }
}