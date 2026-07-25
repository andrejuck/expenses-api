using MongoDB.Bson.Serialization.Attributes;

namespace Transactions.Api.PresentationContracts.Expenses;

public class GroupedExpensesResponse
{
    [BsonId]
    public DateTime Date { get; set; }
    public decimal TotalPrice { get; set; }
    public int Amount { get; set; }
}