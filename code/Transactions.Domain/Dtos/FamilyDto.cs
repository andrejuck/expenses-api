using MongoDB.Bson.Serialization.Attributes;

namespace Transactions.Domain.Dtos;

public class FamilyDto
{
    public required Guid Id { get; set; }
    [BsonElement("Name")]
    public required string FamilyName { get; set; }
    [BsonElement("OwnerUserName")]
    public required string OwnerUserName { get; set; }
    public required IEnumerable<UserDto> Members { get; set; } = [];
    public required IEnumerable<AccountDto> Accounts { get; set; } = [];
}