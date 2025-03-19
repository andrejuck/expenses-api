using Expenses.Domain.Models.Enum;
using Expenses.Infra;
using Expenses.Infra.Settings;
using Libs.Api.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Expenses.Api.ExtensionMethods;

public static class DbContextExtension
{

    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, MongoDbSettings settings)
    {
        var s = BsonClassMap.IsClassMapRegistered(typeof(EnumStringSerializer<UserRole>));
        var ass = BsonSerializer.LookupSerializer(typeof(EnumStringSerializer<UserRole>));

        BsonSerializer.TryRegisterSerializer(typeof(UserRole), new EnumStringSerializer<UserRole>());
        BsonSerializer.TryRegisterSerializer(typeof(RegistrationStatus), new EnumStringSerializer<RegistrationStatus>());
        BsonSerializer.TryRegisterSerializer(typeof(PaymentType), new EnumStringSerializer<PaymentType>());

        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, t => true);
        var dbContext = new DBContext(settings.ConnectionString, settings);
        services.AddSingleton(dbContext);

        dbContext.InitializeData();

        return services;
    }
}