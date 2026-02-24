using Expenses.Api.PresentationContracts.Expenses;
using Transactions.Tests.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using System.Net;
using Transactions.Tests.Mock;

namespace Transactions.Tests.Controller.Expenses;

internal class ExpensePatchPutIntegrationTests : BaseExpenseIntegrationTests
{
    [Test]
    public async Task Should_Update_New_Expense()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var payment2 = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var existingExpense = MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment);
        var patchRequest = new ExpenseForm
        {
            Description = "Cash2",
            ExpenseCategories = new List<string> { "test2" },
            Location = "test2",
            TotalPrice = 11,
            TransactionDate = DateTime.Now.AddDays(1).ToUniversalTime(),
            PaymentMethodId = payment2.Id
        };
        var patch = new JsonPatchDocument<ExpenseForm>();
        patch.Replace(x => x.Description, patchRequest.Description);
        patch.Replace(x => x.Location, patchRequest.Location);
        patch.Replace(x => x.ExpenseCategories, patchRequest.ExpenseCategories);
        patch.Replace(x => x.TotalPrice, patchRequest.TotalPrice);
        patch.Replace(x => x.TransactionDate, patchRequest.TransactionDate);
        patch.Replace(x => x.PaymentMethodId, patchRequest.PaymentMethodId);
        var body = patch.Operations.BuildJsonContent("application/json-patch+json");

        var result = await Client.PatchAsync(BaseUri.Path + $"{existingExpense.Id}", body);

        Assert.That(result.IsSuccessStatusCode, Is.True);
        var createdExpense = FindByDesc("Cash2").FirstOrDefault();
        Assert.NotNull(createdExpense);
        Assert.That(createdExpense.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(createdExpense.PaymentMethodId, Is.EqualTo(patchRequest.PaymentMethodId));
        Assert.That(createdExpense.Description, Is.EqualTo(patchRequest.Description));
        Assert.That(createdExpense.Location, Is.EqualTo(patchRequest.Location));
        Assert.That(createdExpense.TransactionDate.Date, Is.EqualTo(patchRequest.TransactionDate.Date));
        Assert.That(createdExpense.TotalPrice, Is.EqualTo(patchRequest.TotalPrice));
        Assert.That(createdExpense.ExpenseCategories, Is.EqualTo(patchRequest.ExpenseCategories));
    }

    [Test]
    public async Task Should_Not_Update_Expense_PaymentMethod_Not_Found()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);

        MockUser("newuser", "test", UserRole.GeneralUser);
        Authenticate("newuser");

        var existingExpense = MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment);
        var patch = new JsonPatchDocument<ExpenseForm>();
        patch.Replace(x => x.Description, "Cash2");
        var body = patch.Operations.BuildJsonContent("application/json-patch+json");

        var result = await Client.PatchAsync(BaseUri.Path + $"{existingExpense.Id}", body);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Should_Not_Update_Expense_Not_Found()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var existingExpense = MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment);

        MockUser("newuser", "test", UserRole.GeneralUser);
        Authenticate("newuser");

        var patch = new JsonPatchDocument<ExpenseForm>();
        patch.Replace(x => x.Description, "Cash2");
        var body = patch.Operations.BuildJsonContent("application/json-patch+json");

        var result = await Client.PatchAsync(BaseUri.Path + $"{existingExpense.Id}", body);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}

