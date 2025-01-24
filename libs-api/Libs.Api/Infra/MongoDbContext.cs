using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Libs.Api.Infra;

public abstract class MongoDbContext {
        public MongoClient Client { get; set; }
        public IMongoDatabase Database { get; set; }

        public MongoDbContext(string connectionString, string dbName)
        {
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            Client = new MongoClient(settings);
            Database = Client.GetDatabase(dbName);

            InitializeCollections();
        }

        protected abstract void InitializeCollections();
}