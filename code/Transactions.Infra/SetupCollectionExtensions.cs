using Libs.Auth.Models;
using MongoDB.Driver;
using Transactions.Domain.Models;
using Transactions.Domain.Models.Accounts;
using Transactions.Domain.Models.Families;
using Transactions.Domain.Models.PaymentMethod;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Infra;

public static class SetupCollectionExtensions
{
    internal static IMongoCollection<User> SetupCollection(this IMongoCollection<User> collection)
    {
        var indexes = new List<CreateIndexModel<User>>
        {
            new(Builders<User>.IndexKeys.Ascending(x => x.Email)),
            new(Builders<User>.IndexKeys.Ascending(x => x.DeletedAt)),
        };
        
        collection.Indexes.CreateMany(indexes);
        return collection;
    }
    
    internal static IMongoCollection<Module> SetupCollection(this IMongoCollection<Module> collection)
    {
        var indexes = new List<CreateIndexModel<Module>>
        {
            new(Builders<Module>.IndexKeys.Ascending(x => x.Name))
        };
        
        collection.Indexes.CreateMany(indexes);
        return collection;
    }
    
    internal static IMongoCollection<PaymentMethod> SetupCollection(this IMongoCollection<PaymentMethod> collection)
    {
        var indexes = new List<CreateIndexModel<PaymentMethod>>
        {
            new(Builders<PaymentMethod>.IndexKeys.Ascending(x => x.Name)),
            new(Builders<PaymentMethod>.IndexKeys.Ascending(x => x.PaymentType)),
            new(Builders<PaymentMethod>.IndexKeys.Ascending(x => x.DeletedAt)),
        };
        
        collection.Indexes.CreateMany(indexes);
        return collection;
    }
    
    internal static IMongoCollection<Transaction> SetupCollection(this IMongoCollection<Transaction> collection)
    {
        var indexes = new List<CreateIndexModel<Transaction>>
        {
            new(Builders<Transaction>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(field => field.PaymentMethod)
            ),
            new(Builders<Transaction>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(field => field.TransactionDate)
            ),
            new(Builders<Transaction>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(field => field.TransactionType)
            ),
            new(Builders<Transaction>.IndexKeys.Ascending(x => x.DeletedAt)),
        };
        
        collection.Indexes.CreateMany(indexes);
        return collection;
    }
    
    internal static IMongoCollection<Account> SetupCollection(this IMongoCollection<Account> collection)
    {
        var indexes = new List<CreateIndexModel<Account>>
        {
            new(Builders<Account>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(field => field.Name)
            ),
            new(Builders<Account>.IndexKeys.Ascending(x => x.DeletedAt)),
        };
        
        collection.Indexes.CreateMany(indexes);
        return collection;
    }
    
    internal static IMongoCollection<Family> SetupCollection(this IMongoCollection<Family> collection)
    {
        var indexes = new List<CreateIndexModel<Family>>
        {
            new(Builders<Family>.IndexKeys
                .Ascending(x => x.OwnerUserId)
                .Ascending(field => field.Name)
            ),
            new(Builders<Family>.IndexKeys.Ascending(x => x.DeletedAt)),
        };
        
        collection.Indexes.CreateMany(indexes);
        return collection;
    }
}