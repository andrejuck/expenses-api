using Expenses.Api.ExtensionMethods;
using Expenses.Infra;
using Expenses.Infra.Settings;
using Libs.Api.Infra;
using Libs.Api.Serializers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Expenses.Tests.Generics;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private MongoDbRunner _mongoRunner;
    public DBContext DbContext {get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Inicializar o Mongo2Go antes de configurar os serviços
        _mongoRunner = MongoDbRunner.Start();
        var mongoSettings = new MongoDbSettings() { DbName = "TestDB" };
        DbContext = new DBContext(_mongoRunner.ConnectionString, mongoSettings);

        builder.ConfigureServices(services =>
        {
            // Substituir o serviço de IMongoClient pela instância do Mongo2Go
            //BsonSerializer.TryRegisterSerializer(new EnumStringSerializer<UserRole>());
            services.RemoveAll<IMongoClient>();
            services.RemoveAll<MongoDbContext>();
            // BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            services.AddScoped(ssp => DbContext);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // Finalizar o Mongo2Go ao encerrar o teste
        if (disposing)
        {
            _mongoRunner?.Dispose();
        }
    }
}