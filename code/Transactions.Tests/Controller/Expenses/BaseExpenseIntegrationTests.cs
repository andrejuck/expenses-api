
using Transactions.Domain.Models;
using MongoDB.Driver;
using Transactions.Tests.Generics;

namespace Transactions.Tests.Controller.Expenses;

public abstract class BaseExpenseIntegrationTests : BaseIntegrationTest
{
    protected UriBuilder BaseUri = new UriBuilder("http://localhost/api/expense/");

    [SetUp]
    protected override void Setup()
    {
        base.Setup();
        Authenticate();
    }

    [TearDown]
    protected override void Dispose()
    {
        BaseUri.Query = string.Empty;
        base.Dispose();
    }

    protected virtual List<Expense> FindByDesc(string desc)
    {
        return Factory.DbContext.Expenses.Find(Builders<Expense>.Filter.Eq(x => x.Description, desc)).ToList();
    }
}
