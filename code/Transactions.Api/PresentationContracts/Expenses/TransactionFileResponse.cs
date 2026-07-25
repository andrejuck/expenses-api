using Transactions.Api.PresentationContracts.PaymentMethods;


namespace Transactions.Api.PresentationContracts.Expenses;

public class TransactionFileResponse
{
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; }
    public decimal TotalPrice { get; set; }
    public string Location { get; set; }
    public List<string> ExpenseCategories { get; set; }
    public PaymentMethodFileResponse PaymentMethod { get; set; }
}