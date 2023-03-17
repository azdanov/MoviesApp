namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var userId = context.User.Claims
            .SingleOrDefault(claim => claim.Type == AuthConstants.UserIdClaimName)?.Value;

        if (Guid.TryParse(userId, out var parsedId))
        {
            return parsedId;
        }

        return null;
    }
}