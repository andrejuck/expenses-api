using Expenses.Infra;
using Expenses.Infra.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Expenses.Api.ExtensionMethods;

public static class DbContextExtension {

    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, MongoDbSettings settings) {
        // BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        
        var conString = settings.Uri
            .Replace("__username__", settings.Username)
            .Replace("__password__", settings.Password)
            .Replace("__db__", settings.DbName);
        services.AddSingleton(new DBContext(conString, settings));

        return services;
    }
}