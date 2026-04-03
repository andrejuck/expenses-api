using Transactions.Domain.Models.Enum;

namespace Transactions.Api.PresentationContracts.Accounts;

public class AccountForm
{
    public required string AccountName { get; set; }
    public required AccountType AccountType { get; set; }
    public decimal? InitialBalance { get; set; }
}