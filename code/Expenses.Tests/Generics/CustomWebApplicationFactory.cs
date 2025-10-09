using Expenses.Api.ExtensionMethods;
using Expenses.Infra;
using Expenses.Infra.Settings;
using Libs.Api.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Mongo2Go;
using MongoDB.Driver;

namespace Expenses.Tests.Generics;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private MongoDbRunner _mongoRunner;
    public DBContext DbContext { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DOTNET_INTEGRATION_TESTS", "true");
        _mongoRunner = MongoDbRunner.Start();
        var mongoSettings = new MongoDbSettings() { DbName = "TestDB" };
        DbContext = new DBContext(_mongoRunner.ConnectionString, mongoSettings);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMongoClient>();
            services.RemoveAll<MongoDbContext>();
            services.RemoveAll<IStringLocalizer>();

            DbContextExtension.RegisterMongoSerializers();

            services.AddScoped(ssp => DbContext);
            DbContext.InitializeData();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _mongoRunner?.Dispose();
        }
    }
}