SCHERER API

DESCRIPTION
ASP.NET Core 9 Web API for christianscherer.dev. Provides JWT authentication, CRUD for Projects, FluentValidation, EF Core (SQLite for development, PostgreSQL for production), Swagger UI, and a health endpoint.

FEATURES
Projects CRUD (list, create, update, delete).
JWT auth (HS256) for a single admin user with short-lived tokens.
Server-side validation using FluentValidation.
Entity Framework Core with SQLite in development and PostgreSQL (Neon) in production.
CORS configured for the site.
Swagger UI at /swagger and health at /health.
Docker image for production.

TECH
ASP.NET Core 9 and C#.
EF Core with SQLite and Npgsql providers.
FluentValidation.
Swashbuckle for Swagger.

DOMAIN MODEL (PROJECT)
Example JSON:
{"id":"string","title":"string","blurb":"string","tech":["C#","React"],"year":2024,"role":"Solutions Architect","link":"https://example.com","repo":"https://github.com/..."}

Notes: id is a slug. link and repo are optional. tech is a list of strings.

AUTHENTICATION
POST /api/auth/login with body {"password":"yourAdminPassword"} returns {"token":"jwt"}.
Include tokens on protected endpoints using header:
Authorization: Bearer yourJwtHere
Admin password is stored as a BCrypt hash. The JWT key is a 64-byte random secret (Base64) used to sign tokens. Do not place secrets in JWT claims. Always use HTTPS.

ENDPOINTS
GET /health
GET /api/projects
GET /api/projects/{id}
POST /api/projects
PUT /api/projects/{id}
DELETE /api/projects/{id}
POST /api/auth/login

QUICK CALLS (POWERSHELL)
Login:
curl -sX POST https://localhost:7210/api/auth/login
 -H "Content-Type: application/json" -d "{"password":"yourAdminPassword"}"

Create:
curl -sX POST https://localhost:7210/api/projects
 -H "Authorization: Bearer yourJwtHere" -H "Content-Type: application/json" -d "{"title":"Portfolio API","blurb":"ASP.NET Core + EF Core + JWT","tech":["C#",".NET","EF Core"],"year":2025,"role":"Developer","link":"https://christianscherer.dev\"}
"

(If you are on bash, replace the backslash-escaped quotes accordingly.)

LOCAL DEVELOPMENT (SQLITE)
Requirement: .NET 9 SDK installed.
Change directory to the API:
cd .\src\Scherer.Api
Set development secrets (User Secrets):
dotnet user-secrets set "Admin:PasswordHash" "yourBCryptHash"
dotnet user-secrets set "Jwt:Key" "yourBase64Key"
Optional:
dotnet user-secrets set "Jwt:Issuer" "scherer-api"
dotnet user-secrets set "Jwt:Audience" "scherer-site"
Run the API with HTTPS:
dotnet run --launch-profile https
Swagger UI at https://localhost:7210/swagger

Health at https://localhost:7210/health

Development seed data is added only in Development. SQLite file path: Data\app.db.

JWT KEY GENERATOR (POWERSHELL)
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
[Convert]::ToBase64String($bytes)

DATABASE AND MIGRATIONS
The app runs db.Database.Migrate() on startup.

SQLite development migrations:
dotnet ef migrations add DevChange -o Data/Migrations
dotnet ef database update

PostgreSQL production migrations (scaffold against Postgres):
Set temporary environment variables in the current shell:
$env:Database__Provider = "Postgres"
$env:ConnectionStrings__Postgres = "Host=HOST;Database=DB;Username=USER;Password=PASS;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;"
Create a migration and optionally apply it:
dotnet ef migrations add InitialPg -o Data/Migrations
dotnet ef database update
Remove temp env vars:
Remove-Item Env:Database__Provider, Env:ConnectionStrings__Postgres -ErrorAction SilentlyContinue

CORS
Allowed origins by default:
http://localhost:5173

https://christianscherer.dev

To add more, edit the CORS policy in Program.cs and redeploy.

CONFIGURATION
Values may come from appsettings files, User Secrets (development), or environment variables (production). Environment variables use double underscores.
ASPNETCORE_ENVIRONMENT: Development or Production
Database:Provider: Sqlite or Postgres
ConnectionStrings:Sqlite: Data Source=Data/app.db
ConnectionStrings:Postgres: Host=HOST;Database=DB;Username=USER;Password=PASS;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;
Admin:PasswordHash: the BCrypt hash of the admin password
Jwt:Key: a Base64 64-byte secret for HS256
Jwt:Issuer: scherer-api
Jwt:Audience: scherer-site

DOCKER (PRODUCTION IMAGE)
Build:
docker build -t scherer-api .
Run (example):
docker run -p 8080:8080 --env-file .env scherer-api
The container listens on port 8080 (ASPNETCORE_URLS set to http://0.0.0.0:8080
).

DEPLOY (RENDER AND NEON)
Create a Neon Postgres database and use the direct (non-pooler) host.
Create a Render Web Service (Docker) pointing at this repo. Set these environment variables:
ASPNETCORE_ENVIRONMENT=Production
Database__Provider=Postgres
ConnectionStrings__Postgres=Host=HOST;Database=DB;Username=USER;Password=PASS;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;
Jwt__Key=yourBase64Key
Jwt__Issuer=scherer-api
Jwt__Audience=scherer-site
Admin__PasswordHash=yourBCryptHash
Render will build the image and run migrations on boot. Point the website to the API with Vercel env var: VITE_API_URL=https://your-service.onrender.com

SECURITY NOTES
Do not commit secrets. Use User Secrets in development and environment variables in production. Rotate the JWT key to invalidate all tokens. JWTs are signed, not encrypted. Do not include secrets in JWT claims. Always use HTTPS.

TROUBLESHOOTING
PendingModelChangesWarning on startup: you changed the model without adding a migration. Create and apply a migration and commit it.
401 or 403 in Swagger: paste only the raw token (no Bearer prefix) into the Authorize modal. Swagger adds Bearer for you.
CORS errors: add your origin to the CORS policy and redeploy.
Neon connection errors: use the direct host (no -pooler) and include SSL Mode=Require;Trust Server Certificate=true.

LICENSE
Private portfolio project. © Christian Scherer.
