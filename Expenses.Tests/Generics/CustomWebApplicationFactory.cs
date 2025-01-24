
using System.Data.Common;
using Expenses.Infra;
using Expenses.Infra.Settings;
using Libs.Api.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
            services.RemoveAll<IMongoClient>();
            // BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            services.AddSingleton(DbContext);
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