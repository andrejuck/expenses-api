using Libs.Auth.Models;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Domain.Models.Families;

public class Family(
    string name,
    Guid ownerUser,
    IEnumerable<User> members,
    IEnumerable<Account> accounts)
    : BaseEntity
{
    public string Name { get; set; } = name;
    public Guid OwnerUser { get; set; } = ownerUser;
    public IEnumerable<User> Members { get; set; } = members;
    public IEnumerable<Account> Accounts { get; set; } = accounts;
}