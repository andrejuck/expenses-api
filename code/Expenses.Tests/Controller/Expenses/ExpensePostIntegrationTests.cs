using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Tests.Helpers;
using Expenses.Tests.Mock;
using Libs.Api.ErrorHandling.Exceptions;
using Libs.Api.ErrorHandling.Model;
using System.Net;

namespace Expenses.Tests.Controller.Expenses;

internal class ExpensePostIntegrationTests : BaseExpenseIntegrationTests
{
    [Test]
    public async Task Should_Create_New_Expense()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = "test",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = 10,
            TransactionDate = DateTime.Now.ToUniversalTime()
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, content);

        Assert.That(result.IsSuccessStatusCode, Is.True);
        var createdExpense = FindByDesc(form.Description).FirstOrDefault();
        Assert.NotNull(createdExpense);
        Assert.That(createdExpense.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(createdExpense.PaymentMethodId, Is.EqualTo(payment.Id));
        Assert.That(createdExpense.Description, Is.EqualTo(form.Description));
        Assert.That(createdExpense.Location, Is.EqualTo(form.Location));
        Assert.That(createdExpense.TransactionDate.Date, Is.EqualTo(form.TransactionDate.Date));
        Assert.That(createdExpense.TotalPrice, Is.EqualTo(form.TotalPrice));
        Assert.That(createdExpense.ExpenseCategories, Is.EqualTo(form.ExpenseCategories));
    }

    [Test]
    public async Task Should_Not_Create_Expense_Invalid_TotalPrice()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = "test",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = -1,
            TransactionDate = DateTime.Now.ToUniversalTime()
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, content);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Should_Not_Create_Expense_Invalid_Description()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = "",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = 10,
            TransactionDate = DateTime.Now.ToUniversalTime()
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, content);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Should_Not_Create_Expense_Invalid_PaymentType_With_Installment()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = "aaa",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = 10,
            TransactionDate = DateTime.Now.ToUniversalTime(),
            Installment = 2
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, content);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Should_Not_Create_New_Expense_PaymentMethod_Not_Found()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);

        MockUser("newuser", "test", UserRole.GeneralUser);
        Authenticate("newuser");

        var form = new ExpenseForm()
        {
            Description = "test",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = 10,
            TransactionDate = DateTime.Now.ToUniversalTime()
        };
        var content = form.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, content);

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    [TestCase(0)]
    [TestCase(-1)]
    public async Task Should_Return_DomainException_Invalid_TotalPrice_BadRequest(int totalPrice)
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = "test",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = totalPrice,
            TransactionDate = DateTime.Now.ToUniversalTime()
        }.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, form);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<ErrorResponse>();

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(content.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(content.Message, Is.EqualTo("TotalPrice should be higher than 0."));
        Assert.That(content.Action, Is.EqualTo(nameof(DomainException)));
    }

    [Test]
    public async Task Should_Return_DomainException_Invalid_Description_BadRequest()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = string.Empty,
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = 10,
            TransactionDate = DateTime.Now.ToUniversalTime()
        }.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, form);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<ErrorResponse>();

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(content.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(content.Message, Is.EqualTo("Description should be filled."));
        Assert.That(content.Action, Is.EqualTo(nameof(DomainException)));
    }

    [Test]
    public async Task Should_Return_DomainException_Invalid_Installments_BadRequest()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var form = new ExpenseForm()
        {
            Description = "desc",
            ExpenseCategories = new List<string>() { "testCat" },
            PaymentMethodId = payment.Id,
            Location = "test",
            TotalPrice = 10,
            TransactionDate = DateTime.Now.ToUniversalTime(),
            Installment = 10
        }.BuildJsonContent();

        var result = await Client.PostAsync(BaseUri.Path, form);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<ErrorResponse>();

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(content.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(content.Message, Is.EqualTo("Expense installments is allowed to credit cards only."));
        Assert.That(content.Action, Is.EqualTo(nameof(DomainException)));
    }
}
