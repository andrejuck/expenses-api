using System.Net;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;
using Expenses.Tests.Generics;
using Expenses.Tests.Helpers;
using Expenses.Tests.Mock;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Driver;

namespace Expenses.Tests.Controller;

[TestFixture]
public class ExpenseControllerIntegrationTests : BaseIntegrationTest
{
    private UriBuilder BaseUri = new UriBuilder("http://localhost/api/expense/");

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        Authenticate();
    }

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
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
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
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
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
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
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

    [TestCase]
    public async Task Should_Fetch_User_Expenses_Paged_Size_Equals_ExpensesCount()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var existingExpense = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 5 };
        BaseUri.Query =  request.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.Result.Count(), Is.EqualTo(5));
        Assert.True(content.Result.All(x => x.PaymentMethod.Id == payment.Id));
    }

    [Test]
    public async Task Should_Fetch_User_Expenses_Paged_Size_LessThan_ExpensesCount()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var existingExpense = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2 };
        BaseUri.Query =  request.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.Result.Count(), Is.EqualTo(request.PageSize));
        Assert.That(content.TotalPages, Is.EqualTo(3));
    }

    //TODO - Scenario with expenses of multiple users

    private List<Expense> FindByDesc(string desc)
    {
        return Factory.DbContext.Expenses.Find(Builders<Expense>.Filter.Eq(x => x.Description, desc)).ToList();
    }
}