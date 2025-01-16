using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class MongoDbContext {
        public MongoClient Client { get; set; }
        public IMongoDatabase Database { get; set; }

        public MongoDbContext(string connectionString, string dbName)
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            Client = new MongoClient(settings);
            Database = Client.GetDatabase(dbName);

            InitializeCollections();
        }

        protected abstract void InitializeCollections();
}