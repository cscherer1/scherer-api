using Scherer.Api.Features.Projects.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


static SymmetricSecurityKey BuildSigningKey(IConfiguration config)
{
    var raw = config["Jwt:Key"] ?? throw new InvalidOperationException(
        "Jwt:Key is missing. Set it via user-secrets (Development) or env var (Production).");

    // Accept either base64 or plain text; prefer base64 for long random keys
    byte[] keyBytes;
    try
    {
        keyBytes = Convert.FromBase64String(raw);
    }
    catch
    {
        keyBytes = Encoding.UTF8.GetBytes(raw);
    }

    if (keyBytes.Length < 32) // 256 bits minimum for HS256
        throw new InvalidOperationException(
            $"Jwt:Key too short ({keyBytes.Length} bytes). It must be at least 32 bytes.");

    return new SymmetricSecurityKey(keyBytes);
}


var builder = WebApplication.CreateBuilder(args);

// MVC controllers + Swagger (OpenAPI)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Scherer.Api", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: {your JWT token}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };

    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [scheme] = Array.Empty<string>()
    });
});

// CORS: allow site + local dev (to be adjusted later as needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p
        .WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "https://christianscherer.dev",
            "https://www.christianscherer.dev")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddHealthChecks()
    .AddCheck("jwt-key", () =>
    {
        try
        {
            var _ = BuildSigningKey(builder.Configuration);
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message);
        }
    });

builder.Services.AddSingleton<IProjectsRepository, InMemoryProjectsRepository>();

// === JWT Auth ===
var issuer = builder.Configuration["Jwt:Issuer"] ?? "scherer-api";
var audience = builder.Configuration["Jwt:Audience"] ?? "scherer-site";
var signingKey = BuildSigningKey(builder.Configuration); // from earlier fail-fast helper

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 5;                 // 5 attempts
        o.Window = TimeSpan.FromMinutes(1); // per minute
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 2;
    });
});


var app = builder.Build();

// Enable Swagger UI (always, for now, to avoid env confusion)
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirect (will warn if no https endpoint, harmless)
app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
