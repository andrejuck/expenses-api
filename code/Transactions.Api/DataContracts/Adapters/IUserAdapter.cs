using Libs.Auth.Models;
using Transactions.Api.PresentationContracts;

namespace Transactions.Api.DataContracts.Adapters;

public interface IUserAdapter
{
    UserResponse ConvertToResponse(User domain);
}
