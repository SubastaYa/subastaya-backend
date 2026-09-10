using System.Security.Claims;

namespace Presentation.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? user.FindFirst("sub")?.Value;

            return int.TryParse(claimValue, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("Identificador de usuario no válido o ausente en el token de autenticación.");
        }
    }
}
