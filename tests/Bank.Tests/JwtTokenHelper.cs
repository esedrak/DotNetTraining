using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Bank.Tests;

/// <summary>
/// Generates signed JWT tokens that <see cref="Bank.Api.Middleware.AuthMiddleware"/> will accept.
///
/// Uses the same secret and algorithm as <c>AuthMiddleware.ValidateToken</c> so integration
/// tests exercise the real authentication code path rather than bypassing it.
/// </summary>
public static class JwtTokenHelper
{
    // Must match the default in AuthMiddleware.ValidateToken
    private const string Secret = "change-me-in-production-must-be-32-chars!!";

    /// <summary>
    /// Creates a signed JWT with the given user name and scopes.
    /// </summary>
    /// <param name="userName">Value for the <see cref="ClaimTypes.Name"/> claim.</param>
    /// <param name="scopes">
    ///     Zero or more OAuth scope values (e.g. <c>"accounts:write"</c>).
    ///     Each is added as an individual <c>"scope"</c> claim.
    /// </param>
    /// <returns>A signed JWT string ready for use as a Bearer token.</returns>
    public static string GenerateToken(string userName, params string[] scopes)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName)
        };
        claims.AddRange(scopes.Select(s => new Claim("scope", s)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
