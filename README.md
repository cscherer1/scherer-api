# scherer-api

REST API backend for [christianscherer.dev](https://christianscherer.dev) — a production-deployed ASP.NET Core application that powers the portfolio site's project catalog and admin experience.

**Stack:** ASP.NET Core 9 · C# · EF Core · JWT · FluentValidation · PostgreSQL / SQLite · Docker · Render

---

## Why this project exists

A portfolio is only as credible as the systems behind it. Rather than a static list of projects, this API provides a real content pipeline: an authenticated admin can create, update, and delete projects through a hidden admin panel on the site, and visitors read them through a clean REST API. It demonstrates a complete vertical slice — auth, validation, persistence, API design, and production deployment — end to end.

## Key decisions (and why)

| Decision | Choice | Reasoning |
|---|---|---|
| Auth | JWT (HS256), short-lived tokens | Stateless, no server session storage; the site keeps the token **in memory only**, so it survives nothing and can't be stolen from localStorage. |
| Password storage | BCrypt hash | Industry standard for credential hashing; the raw password never exists at rest. |
| Validation | FluentValidation | Server-side validation with clean separation from controllers; the SPA renders the same messages inline. |
| Database | EF Core, SQLite in dev / PostgreSQL (Neon) in prod | Same codebase, zero provider lock-in; production migrations run on boot. |
| Secrets | User Secrets (dev) / environment variables (prod) | Nothing credential-shaped is ever committed. |
| API shape | REST + Swagger UI | Self-documenting; every endpoint is exercisable from the browser. |
| Packaging | Docker image, deployed on Render | One artifact, same behavior locally and in production. |

## Features

- **Projects CRUD** — list, get by slug, create, update, delete
- **JWT authentication** — single admin user, short-lived HS256 tokens
- **BCrypt-hashed credentials** — never stored or transmitted in plaintext
- **FluentValidation** — first-party validation rules on every write path
- **Rate limiting on login** — brute-force resistance on the one public surface
- **Swagger UI** at `/swagger`, **health check** at `/health`
- **CORS** locked to the known site origins
- **Automatic migrations** on startup in all environments
- **OpenAPI** generated via Swashbuckle

## API reference

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/health` | — | Liveness check |
| GET | `/api/projects` | — | List all projects |
| GET | `/api/projects/{slug}` | — | Get one project |
| POST | `/api/projects` | Bearer | Create a project |
| PUT | `/api/projects/{slug}` | Bearer | Update a project |
| DELETE | `/api/projects/{slug}` | Bearer | Delete a project |
| POST | `/api/auth/login` | — | Exchange admin password for a JWT (rate-limited) |

Project model:

```json
{
  "id": "ouroborn-llm-council",
  "title": "Ouroborn",
  "blurb": "Multi-agent LLM council...",
  "tech": ["Python", "Ollama", "SQLite"],
  "year": 2026,
  "role": "Solutions Architect",
  "link": "https://example.com",
  "repo": "https://github.com/... (optional)"
}
```

## Getting started

**Requires:** .NET 9 SDK.

```bash
cd src/Scherer.Api

# configure secrets (dev only)
dotnet user-secrets set "Admin:PasswordHash" "<bcrypt hash>"
dotnet user-secrets set "Jwt:Key" "<64-byte base64>"

# run with HTTPS + Swagger at https://localhost:7210/swagger
dotnet run --launch-profile https
```

`db.Database.Migrate()` applies the schema on startup in every environment, so `dotnet run` is all you need.

### Generate a JWT signing key

```powershell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

## Configuration

| Key | Purpose |
|---|---|
| `Database__Provider` | `Sqlite` (dev) or `Postgres` (prod) |
| `ConnectionStrings__Sqlite` | Dev database path |
| `ConnectionStrings__Postgres` | Production connection string (SSL required) |
| `Admin__PasswordHash` | BCrypt hash of the admin password |
| `Jwt__Key` | 64-byte Base64 HS256 signing key |
| `Jwt__Issuer` / `Jwt__Audience` | Token claims |

## Deployment

Production runs on **Render** as a Docker web service against **Neon Postgres**:

1. `docker build -t scherer-api .`
2. Set the environment variables above
3. Render builds the image and runs migrations on boot

## Security notes

- Secrets live only in User Secrets (dev) / env vars (prod) — never in the repo
- Rotating `Jwt__Key` invalidates all outstanding tokens instantly
- JWTs are signed, not encrypted — no secrets in claims
- CORS is restricted to the actual site origins
- Login rate-limited; password BCrypt-hashed

## License

Private portfolio project. © Christian Scherer.
