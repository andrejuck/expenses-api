using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.Models;
using Expenses.Tests.Helpers;
using Expenses.Tests.Mock;
using Libs.Api.Models;
using System.Net;

namespace Expenses.Tests.Controller.Expenses;

[TestFixture]
public class ExpenseGetIntegrationTests : BaseExpenseIntegrationTests
{
    [Test]
    public async Task Should_Fetch_User_Expenses_Paged_Size_Equals_ExpensesCount()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var existingExpense = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 5 };
        BaseUri.Query = request.BuildQueryParams();

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
        BaseUri.Query = request.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.Result.Count(), Is.EqualTo(request.PageSize));
        Assert.That(content.TotalPages, Is.EqualTo(3));
    }

    [Test]
    public async Task Should_Fetch_User_Expenses_Filtered_By_Description()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var existingExpense = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2 };
        var searchParam = new ExpenseSearchParam() { Description = "2" };
        BaseUri.Query = request.BuildQueryParams();
        BaseUri.Query += searchParam.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.TotalItems, Is.EqualTo(1));
    }

    [Test]
    public async Task Should_Fetch_User_Expenses_Filtered_By_TransactionDate()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1));
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2 };
        var searchParam = new ExpenseSearchParam() { StartTransactionDate = DateTime.Now.AddDays(-1) };
        BaseUri.Query = request.BuildQueryParams();
        BaseUri.Query += searchParam.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.TotalItems, Is.EqualTo(1));
    }

    [Test]
    public async Task Should_Fetch_User_Expenses_Filtered_By_ExpenseCategories()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2 };
        var searchParam = new ExpenseSearchParam() { ExpenseCategories = new List<string>() { "test" } };
        BaseUri.Query = request.BuildQueryParams();
        BaseUri.Query += searchParam.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.TotalItems, Is.EqualTo(1));
    }

    [Test]
    public async Task Should_Fetch_User_Expenses_Filtered_By_PaymentMethod()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        var payment2 = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment2);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2 };
        var searchParam = new ExpenseSearchParam() { PaymentMethodId = payment2.Id };
        BaseUri.Query = request.BuildQueryParams();
        BaseUri.Query += searchParam.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.TotalItems, Is.EqualTo(1));
        Assert.IsTrue(content.Result.All(x => x.PaymentMethod.Id.Equals(payment2.Id)));
    }

    [Test]
    public async Task Should_Fetch_User_Expenses_With_Other_User_Expense()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        MockUser("newuser", "newuser", UserRole.GeneralUser);
        Authenticate("newuser", "newuser");
        var payment2 = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment2);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 2 };
        var searchParam = new ExpenseSearchParam() { PaymentMethodId = payment2.Id };
        BaseUri.Query = request.BuildQueryParams();
        BaseUri.Query += searchParam.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.TotalItems, Is.EqualTo(1));
        Assert.IsTrue(content.Result.All(x => x.UserId.Equals(AdminUser.Id)));
    }

    [TestCase("Description", "Ascending", "test 0")]
    [TestCase("Description", "Descending", "test 4")]
    public async Task Should_Fetch_User_Expenses_SortedBy_StringField(string sortField, string sortOrder, string expectedDescription)
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        var request = new PagedRequest() { CurrentPage = 1, PageSize = 10, IsSorted = true, SortKey = sortField, SortOrder = sortOrder };
        BaseUri.Query = request.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<PagedResponse<ExpenseResponse>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.TotalItems, Is.EqualTo(5));
        Assert.That(content.Result.First().Description, Is.EqualTo(expectedDescription));
    }

    [Test]
    public async Task Should_Fetch_User_Expense_ById()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var expenses = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);

        var result = await Client.GetAsync(BaseUri.Uri + $"{expenses.First().Id}");
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<ExpenseResponse>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.Id, Is.EqualTo(expenses.First().Id));
        Assert.IsNotNull(content.PaymentMethod);
    }

    [Test]
    public async Task Should_Fetch_User_Expense_Categories()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, categories: new List<string> { "restaurant" });
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, categories: new List<string> { "restaurant" });
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, categories: new List<string> { "food" });
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, categories: new List<string> { "tax", "fixed expense" });
        var result = await Client.GetAsync(new Uri(BaseUri.Uri, "categories"));
        var content = result.Content.ReadAsStringAsync().Result.Deserialize<List<string>>();

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content.Count, Is.EqualTo(4));
        Assert.That(content, Is.EqualTo(new List<string> { "fixed expense", "food", "restaurant", "tax" }));
    }

    [Test]
    public async Task Should_Not_Fetch_User_Expense_ById_NotFound()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        var expenses = MockExpense.CreateMultipleExpenses(5, AdminUser, payment, Factory.DbContext.Expenses);
        MockUser("newuser", "newuser");
        Authenticate("newuser", "newuser");

        var result = await Client.GetAsync(BaseUri.Uri + $"{expenses.First().Id}");

        Assert.That(result.IsSuccessStatusCode, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}

