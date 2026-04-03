namespace Transactions.Api.Dtos;

public class AccountDto
{
    public required Guid AccountId { get; set; }
    public required string AccountName { get; set; }
}