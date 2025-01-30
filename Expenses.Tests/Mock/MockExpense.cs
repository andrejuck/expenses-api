using Expenses.Api.PresentationContracts;
using Expenses.Domain.Models;
using MongoDB.Driver;

namespace Expenses.Tests.Mock;

public static class MockExpense
{
    public static Expense CreateExpense(
        IMongoCollection<Expense> collection,
        UserResponse loggedUser,
        PaymentMethod paymentMethod
    )
    {
        var expense = new Expense("test", "test", 10, DateTime.Now, new List<string>() { "test" }, paymentMethod);
        expense.BindUser(loggedUser.Id);

        collection.InsertOne(expense);

        return expense;
    }
}