using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scherer.Api.Data.Migrations
{
    /// <summary>
    /// Data-only migration: repoints the JobDashboard project's Link to its public case-study page
    /// (the live dashboard sits behind Cloudflare Access, so the old link was un-openable by visitors)
    /// and adds the Ouroborn project entry.
    /// Runs automatically on startup (Program.cs -> MigrateAsync), so Render applies it to Neon on next deploy.
    /// No schema change; INSERT/UPDATE ... ON CONFLICT is valid on both PostgreSQL (prod) and SQLite (dev).
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026200000_CaseStudyProjectLinks")]
    public partial class CaseStudyProjectLinks : Migration
    {
        private static readonly (string Id, string Link)[] LinkUpdates =
        {
            ("job-search-automation-dashboard", "/projects/jobdashboard"),
        };

        private static readonly (string Id, string Title, string Blurb, int Year, string Role, string Link, string[] Tech) OurobornRow =
            ("ouroborn-local-llm-council",
                "Ouroborn — Local LLM Deliberation Council",
                "An original multi-agent system where a panel of local language models answers independently, argues over blind-labeled rounds, and a separate foreman model reads the verdict. Runs entirely on owned hardware via Ollama — no API calls, nothing leaves the machine. Opposite of an API wrapper.",
                2026, "Systems & AI Engineer · Personal",
                "/projects/ouroborn",
                new[] { "Python", "Ollama", "LLM", "Docker", "SQLite", "FastAPI" });

        private static string Q(string s) => "'" + s.Replace("'", "''") + "'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (id, link) in LinkUpdates)
            {
                migrationBuilder.Sql($"""UPDATE "Projects" SET "Link" = {Q(link)} WHERE "Id" = {Q(id)};""");
            }

            var r = OurobornRow;
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
            // Restore the original live-dashboard link; remove the Ouroborn row.
            migrationBuilder.Sql($"""UPDATE "Projects" SET "Link" = 'https://jobdashboard.christianscherer.dev' WHERE "Id" = 'job-search-automation-dashboard';""");
            migrationBuilder.Sql($"""DELETE FROM "Projects" WHERE "Id" = {Q(OurobornRow.Id)};""");
        }
    }
}
