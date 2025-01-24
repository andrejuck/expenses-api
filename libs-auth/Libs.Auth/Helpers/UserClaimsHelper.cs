using System.Security.Claims;
using Libs.Auth.Models.Config;

namespace Libs.Auth.Helpers;

public static class UserClaimsHelper
{

    public static Guid GetUserGuidIdFromClaims(ClaimsPrincipal user, CustomClaimSettings settings)
    {
        if(settings == null) throw new ArgumentNullException($"{nameof(CustomClaimSettings)} should be configured.");
        if(settings.Identity == null) throw new ArgumentNullException($"{nameof(CustomClaimSettings)} should have Identity property.");

        return Guid.Parse(user.Claims.FirstOrDefault(x => x.Type.Equals(settings.Identity)).Value);
    }
}