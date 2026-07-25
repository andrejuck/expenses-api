namespace Transactions.Api.PresentationContracts;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public string RegistrationStatus { get; set; }
    public DateTime? LoggedAt { get; set; }
}