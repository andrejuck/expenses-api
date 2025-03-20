using Expenses.Api.PresentationContracts.PaymentMethods;

namespace Expenses.Api.PresentationContracts.Expenses;

public class ExpenseResponse
{
    public Guid Id { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime TransactionDate { get; set; }
    public List<string> ExpenseCategories { get; set; }
    public PaymentMethodResponse PaymentMethod { get; set; }
    public int? Installment { get; set; }
    public Guid UserId { get; set; }
}