
using MongoDB.Driver;
using Libs.Auth.Models;
using Libs.Api.Infra;
using Expenses.Infra.Settings;
using Expenses.Domain.Models;

namespace Expenses.Infra;
public class DBContext : MongoDbContext
{
    public IMongoCollection<User> Users { get; set; }
    public IMongoCollection<Module> Modules { get; set; }

    public DBContext(string connectionString, MongoDbSettings settings)
        : base(connectionString, settings.DbName)
    {
    }

    protected override void InitializeCollections()
    {
        Users = Database.GetCollection<User>("users");
        Modules = Database.GetCollection<Module>("modules");

        InitializeData();
    }

    private void InitializeData()
    {
        var adminModule = Modules.Find(Builders<Module>.Filter.Eq(x => x.Name, "Admin Module")).FirstOrDefault();
        if (adminModule == null)
            Modules.InsertOne(new Module("Admin Module", UserRole.Admin));
    }
}
