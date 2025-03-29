namespace Expenses.Api.PresentationContracts.Expenses;

public class ExpenseForm
{
    public string Location { get; set; }
    public string Description { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime TransactionDate { get; set; }
    public List<string> ExpenseCategories { get; set; }
    public Guid PaymentMethodId { get; set; }
    public int? Installment { get; set; }
}