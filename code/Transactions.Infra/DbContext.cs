
using Transactions.Domain.Models;
using Libs.Api.Infra;
using Libs.Auth.Models;
using MongoDB.Driver;
using Transactions.Infra.Settings;

namespace Transactions.Infra;
public class DBContext : MongoDbContext
{
    public IMongoCollection<User> Users { get; set; }
    public IMongoCollection<Module> Modules { get; set; }
    public IMongoCollection<PaymentMethod> PaymentMethods { get; set; }
    public IMongoCollection<Expense> Expenses { get; set; }

    public DBContext(string connectionString, MongoDbSettings settings)
        : base(connectionString, settings.DbName)
    {
    }

    protected override void InitializeCollections()
    {
        Users = Database.GetCollection<User>("users");
        Modules = Database.GetCollection<Module>("modules");
        PaymentMethods = Database.GetCollection<PaymentMethod>("payment-methods");
        Expenses = Database.GetCollection<Expense>("expenses");
    }

    public void InitializeData()
    {
        var adminModule = Modules.Find(NameFilter<Module>("Admin Module")).FirstOrDefault();
        if (adminModule == null) Modules.InsertOne(new Module("Admin Module", Guid.NewGuid(), UserRole.Admin));

        var configModule = Modules.Find(NameFilter<Module>("Configuration Module")).FirstOrDefault();
        if (configModule == null) Modules.InsertOne(new Module("Configuration Module", Guid.NewGuid(), UserRole.Admin, UserRole.GeneralUser));
    }

    private FilterDefinition<T> NameFilter<T>(string name)
    {
        return Builders<T>.Filter.Eq("Name", name);
    }
}
