using Expenses.Domain.Models;
using Expenses.Infra;
using Expenses.Infra.Settings;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Expenses.Api.ExtensionMethods;

public static class DbContextExtension
{

    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, MongoDbSettings settings)
    {
        // BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, t => true);

        var conString = settings.Uri
            .Replace("__username__", settings.Username)
            .Replace("__password__", settings.Password)
            .Replace("__db__", settings.DbName);
        services.AddSingleton(new DBContext(conString, settings));

        return services;
    }
}