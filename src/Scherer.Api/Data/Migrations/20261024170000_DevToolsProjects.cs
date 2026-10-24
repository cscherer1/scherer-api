using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scherer.Api.Data.Migrations
{
    /// <summary>
    /// Data-only migration: adds the 2026 developer-tooling projects (Job Search Dashboard, RouteLLM CLI pipeline).
    /// Runs automatically on startup (Program.cs -> MigrateAsync), so Render applies it to Neon on next deploy.
    /// Existing projects are untouched; no schema change, so the model snapshot is unchanged.
    /// INSERT ... ON CONFLICT is valid on both PostgreSQL (prod) and SQLite (dev).
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261024170000_DevToolsProjects")]
    public partial class DevToolsProjects : Migration
    {
        private sealed record Row(string Id, string Title, string Blurb, int Year, string Role, string? Link, string? Repo, string[] Tech);

        private static readonly Row[] Rows =
        {
            new("job-search-automation-dashboard",
                "Job Search Automation Dashboard",
                "Automated job-search pipeline that scrapes live postings daily (JobSpy), builds a verified skill inventory with evidence, and uses LLM-powered evaluation to score fit against real, provable experience before generating tailored resume PDFs and cover letters. Orchestrated with n8n on Docker, data in NocoDB, dashboard in Python/FastAPI.",
                2026, "Systems & AI Engineer · Personal",
                "https://jobdashboard.christianscherer.dev", null,
                new[] { "n8n", "NocoDB", "JobSpy", "Python", "LLM", "Docker", "FastAPI" }),
            new("routellm-cli-development-pipeline",
                "RouteLLM CLI in the Development Pipeline",
                "Integrated the RouteLLM CLI — an OpenAI-compatible LLM routing gateway — into the development pipeline, giving every agentic and automation workflow a single unified endpoint that routes each request to the best model for the job.",
                2026, "Systems & AI Engineer · Personal",
                "https://routellm.abacus.ai", null,
                new[] { "RouteLLM", "LLM", "OpenAI-compatible API", "CLI", "Python", "Automation" }),
        };

        private static string Q(string s) => "'" + s.Replace("'", "''") + "'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var r in Rows)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO "Projects" ("Id", "Title", "Blurb", "Year", "Role", "Link", "Repo", "Tech")
                    VALUES ({Q(r.Id)}, {Q(r.Title)}, {Q(r.Blurb)}, {r.Year}, {Q(r.Role)},
                            {(r.Link is null ? "NULL" : Q(r.Link))}, {(r.Repo is null ? "NULL" : Q(r.Repo))},
                            {Q(JsonSerializer.Serialize(r.Tech))})
                    ON CONFLICT ("Id") DO UPDATE SET
                        "Title" = excluded."Title",
                        "Blurb" = excluded."Blurb",
                        "Year"  = excluded."Year",
                        "Role"  = excluded."Role",
                        "Link"  = excluded."Link",
                        "Repo"  = excluded."Repo",
                        "Tech"  = excluded."Tech";
                    """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var ids = string.Join(", ", Rows.Select(r => Q(r.Id)));
            migrationBuilder.Sql($"""DELETE FROM "Projects" WHERE "Id" IN ({ids});""");
        }
    }
}
