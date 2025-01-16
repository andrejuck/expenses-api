namespace Expenses.Api.PresentationContracts;

public class UserResponse {

    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public RegistrationStatus RegistrationStatus { get; set; }
    public DateTime LoggedAt { get; set; }
}