using Libs.Auth.Models.Config;
using System.Security.Claims;

namespace Libs.Auth.Helpers;

public static class UserClaimsHelper
{

    public static Guid GetUserGuidIdFromClaims(ClaimsPrincipal user, CustomClaimSettings settings)
    {
        if (settings is null) throw new ArgumentNullException($"{nameof(CustomClaimSettings)} should be configured.");
        if (settings.Identity is null) throw new ArgumentNullException($"{nameof(CustomClaimSettings)} should have Identity property.");
        if (user is null) throw new UnauthorizedAccessException();
        if (user.Claims is null || user.Claims.Count() == 0) throw new UnauthorizedAccessException();

        return Guid.Parse(user.Claims.FirstOrDefault(x => x.Type.Equals(settings.Identity)).Value);
    }
}