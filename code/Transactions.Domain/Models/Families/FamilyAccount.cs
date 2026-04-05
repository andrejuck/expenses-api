using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Models.Families;

public class FamilyAccount
{
    public required Guid Id { get; set; }
    public required string AccountName { get; set; }
    public AccountType AccountType { get; set; }
}