using Transactions.Domain.Models;
using MongoDB.Driver;
using System.Net;
using Transactions.Tests.Mock;

namespace Transactions.Tests.Controller.Expenses;

[TestFixture]
public class ExpenseDeleteIntegrationTests : BaseExpenseIntegrationTests
{
    [Test]
    public async Task Should_Delete_User_Expense()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var expenses = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);

        var result = await Client.DeleteAsync(BaseUri.Uri + $"{expenses.First().Id}");

        var deleted = Factory.DbContext.Expenses.Find(Builders<Expense>.Filter.Eq(x => x.Id, expenses.First().Id)).First();
        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        Assert.IsNotNull(deleted.DeletedAt);
    }

    [Test]
    public async Task Should_Not_Delete_User_Expense_NotFound()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var expenses = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        MockUser("newuser", "new");
        Authenticate("newuser", "new");

        var result = await Client.DeleteAsync(BaseUri.Uri + $"{expenses.First().Id}");

        var deleted = Factory.DbContext.Expenses.Find(Builders<Expense>.Filter.Eq(x => x.Id, expenses.First().Id)).First();
        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.IsNull(deleted.DeletedAt);
    }
}

