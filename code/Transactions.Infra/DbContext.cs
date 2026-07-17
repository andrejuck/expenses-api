
using Transactions.Domain.Models;
using Libs.Api.Infra;
using Libs.Auth.Models;
using MongoDB.Driver;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;
using Transactions.Domain.Models.Transaction;
using Transactions.Infra.Settings;

namespace Transactions.Infra;
public class DBContext : MongoDbContext
{
    private readonly MongoDbSettings _settings;
    public IMongoCollection<User> Users { get; set; }
    public IMongoCollection<Module> Modules { get; set; }
    public IMongoCollection<PaymentMethod> PaymentMethods { get; set; }
    public IMongoCollection<Transaction> Expenses { get; set; }
    public IMongoCollection<Account> Accounts { get; set; }
    public IMongoCollection<Family> Families { get; set; }

    public DBContext(string connectionString, MongoDbSettings settings)
        : base(connectionString, settings.DbName)
    {
        _settings = settings;
    }

    protected override void InitializeCollections()
    {
        Users = Database.GetCollection<User>("users").SetupCollection();
        Modules = Database.GetCollection<Module>("modules").SetupCollection();
        PaymentMethods = Database.GetCollection<PaymentMethod>("payment-methods").SetupCollection();
        Expenses = Database.GetCollection<Transaction>("expenses").SetupCollection();
        Accounts = Database.GetCollection<Account>("accounts").SetupCollection();
        Families = Database.GetCollection<Family>("families").SetupCollection();
    }

    public void InitializeData()
    {
        var adminModule = Modules.Find(NameFilter<Module>("Admin Module")).FirstOrDefault();
        if (adminModule == null) Modules.InsertOne(new Module("Admin Module", Guid.NewGuid(), UserRole.Admin));

        SetupModules();
        
        var adminUser = Users.Find(Builders<User>.Filter.Eq(x => x.Username, "Admin")).FirstOrDefault();
        if (adminUser is null && _settings.IsDevelopment) SetupAdminUser();
    }

    private void SetupAdminUser()
    {
        var newUser = new User("admin", "Admin");
        newUser.AddRoles(UserRole.Admin);
        newUser.ConfirmEmail();
        newUser.UpdateRegistrationStatus(RegistrationStatus.Approved);
        newUser.SetPassword(_settings.AdminPassword ?? "admin123");
        Users.InsertOne(newUser);
    }

    private void SetupModules()
    {
        var configModule = new Module("Configuration Module", Guid.NewGuid(), UserRole.Admin, UserRole.GeneralUser);
        if (!Modules.Find(NameFilter<Module>(configModule.Name)).Any()) Modules.InsertOne(configModule);
        
        var accountModule = new Module("Account Module", Guid.NewGuid(), UserRole.Admin, UserRole.GeneralUser);
        if (!Modules.Find(NameFilter<Module>(accountModule.Name)).Any()) Modules.InsertOne(accountModule);
        
        var familyModule = new Module("Family Module", Guid.NewGuid(), UserRole.Admin, UserRole.GeneralUser);
        if (!Modules.Find(NameFilter<Module>(familyModule.Name)).Any()) Modules.InsertOne(familyModule);
    }

    private FilterDefinition<T> NameFilter<T>(string name)
    {
        return Builders<T>.Filter.Eq("Name", name);
    }
}
