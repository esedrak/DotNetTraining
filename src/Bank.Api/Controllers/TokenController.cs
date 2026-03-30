using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Bank.Api.Controllers;

/// <summary>
/// Issues signed JWTs for local development and testing only.
/// Returns 404 in any non-Development environment.
/// </summary>
[ApiController]
[Route("v1/token")]
public class TokenController(IHostEnvironment env, IConfiguration config) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        if (!env.IsDevelopment())
        {
            return NotFound();
        }

        var secret = config["Jwt:Secret"] ?? "change-me-in-production-must-be-32-chars!!";
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> { new(ClaimTypes.Name, request.UserName) };
        claims.AddRange(request.Scopes.Select(s => new Claim("scope", s)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token)));
    }
}

public record TokenRequest(string UserName, string[] Scopes);
public record TokenResponse(string Token);
