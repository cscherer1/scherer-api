using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scherer.Api.Data.Migrations
{
    /// <summary>
    /// Data-only migration: adds the JobDashboard project to the portfolio.
    /// Runs automatically on startup (Program.cs -> MigrateAsync), so Render applies it to Neon on next deploy.
    /// Existing projects are untouched; no schema change, so the model snapshot is unchanged.
    /// INSERT ... ON CONFLICT is valid on both PostgreSQL (prod) and SQLite (dev).
    /// Mirrors the scherer-site content.ts fallback entry (id job-search-automation-dashboard) so the
    /// API and the fallback show the same project.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026183000_JobDashboardProject")]
    public partial class JobDashboardProject : Migration
    {
        private static readonly (string Id, string Title, string Blurb, int Year, string Role, string Link, string[] Tech) Row =
            ("job-search-automation-dashboard",
                "JobDashboard — Self-Hosted Career Automation Stack",
                "Automated job-search pipeline that scrapes live postings daily (JobSpy), builds a verified skill inventory with evidence, and uses LLM-powered evaluation to score fit against real, provable experience before generating tailored resume PDFs and cover letters. Orchestrated with n8n on Docker, data in NocoDB, dashboard in Python/FastAPI. Architected around the Cloudflare 524 edge-timeout lesson: heavy work runs as background jobs that report progress instead of one long-lived request.",
                2026, "Systems & AI Engineer · Personal",
                "https://jobdashboard.christianscherer.dev",
                new[] { "n8n", "NocoDB", "JobSpy", "Python", "LLM", "Docker", "FastAPI" });

        private static string Q(string s) => "'" + s.Replace("'", "''") + "'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var r = Row;
            migrationBuilder.Sql($"""
                INSERT INTO "Projects" ("Id", "Title", "Blurb", "Year", "Role", "Link", "Repo", "Tech")
                VALUES ({Q(r.Id)}, {Q(r.Title)}, {Q(r.Blurb)}, {r.Year}, {Q(r.Role)}, {Q(r.Link)}, NULL, {Q(JsonSerializer.Serialize(r.Tech))})
                ON CONFLICT ("Id") DO UPDATE SET
                    "Title" = excluded."Title",
                    "Blurb" = excluded."Blurb",
                    "Year"  = excluded."Year",
                    "Role"  = excluded."Role",
                    "Link"  = excluded."Link",
                    "Tech"  = excluded."Tech";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""DELETE FROM "Projects" WHERE "Id" = {Q(Row.Id)};""");
        }
    }
}
