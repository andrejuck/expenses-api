using Expenses.Domain.Models;
using Expenses.Domain.Models.Enum;
using Expenses.Infra;
using Expenses.Infra.Settings;
using Libs.Api.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Expenses.Api.ExtensionMethods;

public static class DbContextExtension
{

    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, MongoDbSettings settings)
    {
        BsonSerializer.RegisterSerializer(typeof(UserRole), new EnumStringSerializer<UserRole>());
        BsonSerializer.RegisterSerializer(typeof(RegistrationStatus), new EnumStringSerializer<RegistrationStatus>());
        BsonSerializer.RegisterSerializer(typeof(PaymentType), new EnumStringSerializer<PaymentType>());

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