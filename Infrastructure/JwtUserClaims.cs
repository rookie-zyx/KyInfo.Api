using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KyInfo.Api.Infrastructure;

internal static class JwtUserClaims
{
    public const string RoleClaim = "role";

    public static int? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub != null && int.TryParse(sub, out var id) ? id : null;
    }

    public static string GetRole(ClaimsPrincipal user)
    {
        return user.FindFirstValue(RoleClaim)
               ?? user.FindFirstValue(ClaimTypes.Role)
               ?? "User";
    }

    public static bool IsStaff(string role) =>
        string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, "Root", StringComparison.OrdinalIgnoreCase);
}
