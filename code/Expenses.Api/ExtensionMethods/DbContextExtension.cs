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
        RegisterMongoSerializers();

        var dbContext = new DBContext(settings.ConnectionString, settings);
        services.AddSingleton(dbContext);

        dbContext.InitializeData();

        return services;
    }

    public static void RegisterMongoSerializers()
    {
        BsonSerializer.TryRegisterSerializer(typeof(UserRole), new EnumStringSerializer<UserRole>());
        BsonSerializer.TryRegisterSerializer(typeof(RegistrationStatus), new EnumStringSerializer<RegistrationStatus>());
        BsonSerializer.TryRegisterSerializer(typeof(PaymentType), new EnumStringSerializer<PaymentType>());

        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, t => true);
    }
}