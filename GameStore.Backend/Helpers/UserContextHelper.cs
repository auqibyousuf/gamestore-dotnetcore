using System.Security.Claims;
using GameStore.Backend.Dtos.Common;

namespace GameStore.Backend.Helpers;

public class UserContextHelper
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User is not authenticated");

        if (!int.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Invalid user id in token");

        return userId;
    }
    public static string GetUserRole(ClaimsPrincipal user)
    {
        var userRole = user.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(userRole))
            throw new InvalidOperationException("Invalid role");
        return userRole;

    }

    public static UserContextDto GetUserContext(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("UserId claim missing");

        if (!int.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Invalid UserId in token");

        // 2️⃣ Role
        var role = user.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(role))
            throw new UnauthorizedAccessException("Role claim missing");

        // 3️⃣ Email
        var email = user.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            throw new UnauthorizedAccessException("Email claim missing");

        return new UserContextDto
        {
            UserId = userId,
            Role = role,
            Email = email
        };
    }
}
