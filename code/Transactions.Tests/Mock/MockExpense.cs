using Transactions.Api.PresentationContracts;
using Transactions.Domain.Models;
using MongoDB.Driver;
using Transactions.Domain.Models.Enum;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Tests.Mock;

public static class MockExpense
{
    public static Transaction CreateExpense(
        IMongoCollection<Transaction> collection,
        UserResponse loggedUser,
        PaymentMethod paymentMethod,
        DateTime? transactionDate = null,
        List<string> categories = null,
        decimal totalPrice = 10
    )
    {
        var expense = new Transaction(
            "test",
            "test",
            totalPrice,
            transactionDate ?? DateTime.Now,
            categories ?? new List<string>() { "test" },
            paymentMethod,
            TransactionType.Expense,
            Guid.NewGuid()
            );
        expense.BindUser(loggedUser.Id);

        collection.InsertOne(expense);

        return expense;
    }

    public static List<Transaction> CreateMultipleExpenses(int count, UserResponse loggedUser, PaymentMethod paymentMethod, IMongoCollection<Transaction> collection)
    {
        var listExpense = new List<Transaction>();
        for (int i = 0; i < count; i++)
        {
            var expense = new Transaction(
                "test", 
                $"test {i}", 
                10 + i,
                DateTime.Now,
                new List<string>() { $"test {i}" },
                paymentMethod, 
                TransactionType.Expense,
                Guid.NewGuid());
            expense.BindUser(loggedUser.Id);

            listExpense.Add(expense);
            collection.InsertOne(expense);
        }

        return listExpense;
    }
}