
namespace Expenses.Domain.Models;

public class ExpenseSearchParam
{
    public string Description { get; set; }
    public DateTime? TransactionDate { get; set; }
    public List<string> ExpenseCategories { get; set; }
    public Guid? PaymentMethodId { get; set; }
}