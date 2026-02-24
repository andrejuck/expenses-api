using Expenses.Api.PresentationContracts.PaymentMethods;

namespace Expenses.Api.PresentationContracts.Expenses;

public class ExpenseResponse
{
    public Guid Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime TransactionDate { get; set; }
    public List<string> ExpenseCategories { get; set; } = [];
    public required PaymentMethodResponse PaymentMethod { get; set; }
    public int? Installment { get; set; }
    public Guid UserId { get; set; }
}