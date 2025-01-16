
using MongoDB.Driver;
using Libs.Auth.Models;
using Libs.Api.Infra;
using Expenses.Infra.Settings;

namespace Expenses.Infra;
    public class DBContext : MongoDbContext
    {
        public IMongoCollection<User> Users { get; set; }

        public DBContext(string connectionString, MongoDbSettings settings)
            : base(connectionString, settings.DbName)
        {
        }

        protected override void InitializeCollections()
        {
            Users = Database.GetCollection<User>("users");
        }
    }
