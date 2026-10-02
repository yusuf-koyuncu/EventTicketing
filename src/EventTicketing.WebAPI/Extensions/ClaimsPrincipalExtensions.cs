using System.Security.Claims;
using EventTicketing.Domain.Exceptions;

namespace EventTicketing.WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var userId)
            ? userId
            : throw new InvalidCredentialsException("The token does not carry a valid user id.");
    }
}
