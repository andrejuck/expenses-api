using Libs.Auth.Models;
using Transactions.Api.DataContracts.Adapters;
using Transactions.Api.PresentationContracts;

namespace Transactions.Api.Adapters;

public class UserAdapter : IUserAdapter
{
    public UserResponse ConvertToResponse(User domain) =>
        new()
        {
            Id = domain.Id,
            Username = domain.Username,
            Email = domain.Email,
            Roles = domain.Roles.Select(role => role.ToString()).ToList() ?? [],
            RegistrationStatus = domain.RegistrationStatus.ToString(),
            LoggedAt = domain.LoggedAt
        };
}
