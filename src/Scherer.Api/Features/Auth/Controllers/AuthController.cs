using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Scherer.Api.Features.Auth.Dtos;
using Microsoft.AspNetCore.RateLimiting;

namespace Scherer.Api.Features.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration config) : ControllerBase
{
    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<object> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Password))
            return Unauthorized("Invalid credentials.");

        // Prefer hashed password if present
        var hash = config["Admin:PasswordHash"];
        if (!string.IsNullOrWhiteSpace(hash))
        {
            if (!BCrypt.Net.BCrypt.Verify(req.Password, hash))
                return Unauthorized("Invalid credentials.");
        }
        else
        {
            // Fallback to plaintext secret ONLY if no hash configured
            var expected = config["Admin:Password"];
            if (string.IsNullOrWhiteSpace(expected) || req.Password != expected)
                return Unauthorized("Invalid credentials.");
        }

        var raw = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set.");
        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(raw);  // prefer Base64 (what we stored)
        }
        catch
        {
            keyBytes = Encoding.UTF8.GetBytes(raw);    // fallback to plain text
        }

        var signingKey = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);


        var issuer = config["Jwt:Issuer"] ?? "scherer-api";
        var audience = config["Jwt:Audience"] ?? "scherer-site";

        var claims = new[]
        {
        new Claim(ClaimTypes.Name, "admin"),
        new Claim(ClaimTypes.Role, "admin"),
    };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = jwt });
    }
}
