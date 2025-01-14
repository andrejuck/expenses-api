
using MongoDB.Driver;
using MongoDB.Bson;
using Libs.Auth.Models;

namespace Expenses.Infra;
    public class DBContext
    {
        public MongoClient Client { get; set; }
        public IMongoDatabase Database { get; set; }
        public IMongoCollection<User> Users { get; set; }

        public DBContext(string connectionString)
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            // settings.ServerApi = new ServerApi(ServerApiVersion.);
            // settings. = GuidRepresentation.Standard

            Client = new MongoClient(settings);
            Database = Client.GetDatabase("expenses");

            InitializeCollections();
        }

        private void InitializeCollections()
        {
            Users = Database.GetCollection<User>("users");
        }
    }
