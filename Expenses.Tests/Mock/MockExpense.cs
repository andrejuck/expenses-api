using Expenses.Api.PresentationContracts;
using Expenses.Domain.Models;
using MongoDB.Driver;

namespace Expenses.Tests.Mock;

public static class MockExpense
{
    public static Expense CreateExpense(
        IMongoCollection<Expense> collection,
        UserResponse loggedUser,
        PaymentMethod paymentMethod,
        DateTime? transactionDate = null,
        List<string> categories = null
    )
    {
        var expense = new Expense("test", "test", 10, transactionDate ?? DateTime.Now, categories ?? new List<string>() { "test" }, paymentMethod);
        expense.BindUser(loggedUser.Id);

        collection.InsertOne(expense);

        return expense;
    }

    public static List<Expense> CreateMultipleExpenses(int count, UserResponse loggedUser, PaymentMethod paymentMethod, IMongoCollection<Expense> collection)
    {
        var listExpense = new List<Expense>();
        for (int i = 0; i < count; i++)
        {
            var expense = new Expense("test", $"test {i}", 10+i, DateTime.Now, new List<string>() { $"test {i}" }, paymentMethod);
            expense.BindUser(loggedUser.Id);

            listExpense.Add(expense);
            collection.InsertOne(expense);
        }

        return listExpense;
    }
}