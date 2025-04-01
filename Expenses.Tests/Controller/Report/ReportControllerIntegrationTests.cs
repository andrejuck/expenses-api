using DnsClient.Protocol;
using Expenses.Api.Adapters;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.Models;
using Expenses.Tests.Generics;
using Expenses.Tests.Helpers;
using Expenses.Tests.Mock;
using MongoDB.Driver.Linq;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Resources;
using System.Text.RegularExpressions;

namespace Expenses.Tests.Controller.Report;
internal class ReportControllerIntegrationTests : BaseIntegrationTest
{
    protected UriBuilder BaseUri = new UriBuilder("http://localhost/api/report/");
    private CsvHelperAdapter _csvAdapter;
    private ResourceManager _resourceManager = new ResourceManager("Expenses.Api.Resources.SharedResources", typeof(Program).Assembly);

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        Authenticate();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        _csvAdapter = new CsvHelperAdapter(Factory.Services);
    }

    [TearDown]
    protected override void Dispose()
    {
        BaseUri = new UriBuilder("http://localhost/api/report/");
        base.Dispose();
    }

    [Test]
    public async Task Should_Fetch_CSV_With_User_Expenses()
    {
        Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(CultureInfo.CurrentCulture.Name));
        var localizationResource = _resourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now, totalPrice: 10.2M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now, totalPrice: 12.2M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now, totalPrice: 20.18M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1), totalPrice: 25.23M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1), totalPrice: 26.37M);

        var result = await Client.GetAsync(BaseUri.Uri + "expenses/csv");
        Assert.That(result.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));

        var records = _csvAdapter.ReadCsv<ExpenseFileResponse, ExpenseMap>(result.Content.ReadAsStringAsync().Result, CultureInfo.CurrentCulture).Records;
        Assert.That(records.Count, Is.EqualTo(5));
        Assert.That(records.GroupBy(x => x.TransactionDate).Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Should_Fetch_CSV_With_User_Expenses_Within_TransactionDate_Range()
    {
        BaseUri.Path += "expenses/csv";
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1), totalPrice: 10.2M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1), totalPrice: 12.2M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1), totalPrice: 20.18M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-5), totalPrice: 25.23M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-5), totalPrice: 26.37M);
        var searchParam = new ExpenseSearchParam() { StartTransactionDate = DateTime.Now.AddDays(-3), EndTransactionDate =  DateTime.Now};
        BaseUri.Query = searchParam.BuildQueryParams();

        var result = await Client.GetAsync(BaseUri.Uri);
        Assert.That(result.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));

        var records = _csvAdapter.ReadCsv<ExpenseFileResponse, ExpenseMap>(result.Content.ReadAsStringAsync().Result).Records;
        Assert.That(records.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Should_Return_No_Content_When_User_Doesnt_Have_Expenses()
    {
        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);

        var result = await Client.GetAsync(BaseUri.Uri + "expenses/csv");

        Assert.That(result.IsSuccessStatusCode, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [TestCase("en")]
    [TestCase("pt-BR")]
    public async Task Should_Fetch_CSV_With_Localized_Headers(string locale)
    {
        var culture = CultureInfo.GetCultureInfo(locale);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(locale));
        var localizationResource = _resourceManager.GetResourceSet(culture, true, true);

        var payment = MockPaymentMethod.CreatePaymentMethod(Factory.DbContext.PaymentMethods, AdminUser);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-1), totalPrice: 25.23M);
        MockExpense.CreateExpense(Factory.DbContext.Expenses, AdminUser, payment, DateTime.Now.AddDays(-2), totalPrice: 26.37M);

        var result = await Client.GetAsync(BaseUri.Uri + "expenses/csv");
        Assert.That(result.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));

        var csvRecords = _csvAdapter.ReadCsv<ExpenseFileResponse, ExpenseMap>(
            result.Content.ReadAsStringAsync().Result,
            culture
            );

        foreach (DictionaryEntry item in localizationResource)
        {
            Assert.That(csvRecords.Headers.Contains(item.Value));
        }

        Assert.That(csvRecords.Records.Count, Is.EqualTo(2));
        Assert.That(csvRecords.Records.GroupBy(x => x.TransactionDate).Count(), Is.EqualTo(2));
    }

}

