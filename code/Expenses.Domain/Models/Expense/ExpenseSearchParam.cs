namespace Expenses.Domain.Models;

public class ExpenseSearchParam
{
    public string Description { get; set; }
    public DateTime? StartTransactionDate { get; set; }
    public DateTime? EndTransactionDate { get; set; }
    public List<string> ExpenseCategories { get; set; }
    public Guid? PaymentMethodId { get; set; }
}