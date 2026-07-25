namespace Transactions.Api.PresentationContracts.Accounts;

public record RecentAccountResponse(Guid AccountId, string AccountName, decimal Balance, DateTime? UpdatedAt);