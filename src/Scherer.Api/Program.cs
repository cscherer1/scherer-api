using Scherer.Api.Features.Projects.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


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
        Description = "Enter: Bearer {your JWT token}",
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
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IProjectsRepository, InMemoryProjectsRepository>();

// === JWT Auth ===
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-super-secret-change-me"; //TODO: set env in prod
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Enable Swagger UI (always, for now, to avoid env confusion)
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirect (will warn if no https endpoint, harmless)
app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
