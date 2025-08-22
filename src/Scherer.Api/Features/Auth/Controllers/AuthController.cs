using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Scherer.Api.Features.Auth.Dtos;

namespace Scherer.Api.Features.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration config) : ControllerBase
{
    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<object> Login([FromBody] LoginRequest req)
    {
        var expected = config["Admin:Password"] ?? "SetYourAdminPassword"; // set ADMIN__PASSWORD in env for real
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password != expected)
            return Unauthorized("Invalid credentials.");

        var key = config["Jwt:Key"] ?? "dev-super-secret-change-me"; // set JWT__KEY in env for real
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "admin"),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = jwt });
    }
}
