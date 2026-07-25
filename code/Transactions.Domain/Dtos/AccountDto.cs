using MongoDB.Bson.Serialization.Attributes;
using Transactions.Domain.Models.Enum;

namespace Transactions.Domain.Dtos;

public class AccountDto
{
    public required Guid Id { get; set; }
    [BsonElement("Name")]
    public required string AccountName { get; set; }
    public required AccountType AccountType { get; set; }
}