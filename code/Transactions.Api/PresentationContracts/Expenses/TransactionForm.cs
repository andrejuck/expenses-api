using Transactions.Domain.Models.Enum;

namespace Transactions.Api.PresentationContracts.Expenses;

public class TransactionForm
{
    public string? Location { get; init; }
    public required string Description { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime TransactionDate { get; init; }
    public List<string>? ExpenseCategories { get; init; }
    public Guid PaymentMethodId { get; init; }
    public int? Installment { get; init; }
    public TransactionType TransactionType { get; init; }
    public Guid? AccountId { get; init; }
}