namespace Transactions.Domain.Models;

public class UserSearchParam
{
    public List<string>? RegistrationStatus { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}