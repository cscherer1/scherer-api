using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scherer.Api.Data.Migrations
{
    /// <summary>
    /// Data-only migration: replaces the uncurated portfolio dump with seven
    /// outcome-focused projects that match the current positioning.
    /// The previous API payload is preserved in scripts/legacy-projects-2026-11-13.json.
    /// No schema change, so the model snapshot is unchanged.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261113224500_CuratedPortfolioProjects")]
    public partial class CuratedPortfolioProjects : Migration
    {
        private sealed record Row(string Id, string Title, string Blurb, int Year, string Role, string? Link, string[] Tech);

        private static readonly Row[] Rows =
        {
            new("enterprise-integration-automation",
                "Enterprise Integration & Automation",
                "Automate enterprise and M365 processes through ServiceNow, Azure and PowerShell: access and licensing changes, IP-address management, backup and restore, and supporting data workflows. Examples include EfficientIP pool reservations, Revit server and ImaginIT Clarity automation, delegated manager access to departed employees' files, and ArcGIS backup and restore in Azure. The work removed hundreds of administrative hours a year and replaced hand-run processes with documented, supported services.",
                2025, "Staff Software Developer · Burns & McDonnell", null,
                new[] { "Azure", "ServiceNow", "PowerShell", "Microsoft 365", "EfficientIP", "ArcGIS", "REST / OAuth" }),
            new("ibmi-azure-integration-bridge",
                "IBM i ↔ Azure Integration Bridge",
                "Design and maintain the bridge between IBM i (RPGLE/CL) business systems and Azure-hosted Python and .NET services. Work with stakeholders to define interfaces, replace manual workflows with supported automation, and keep production integrations observable and recoverable.",
                2026, "Integration Architect / Programmer-Analyst · Murphy-Hoffman", null,
                new[] { "IBM i", "RPGLE", "CL", "Azure", "Python", ".NET", "REST APIs" }),
            new("op-e-console-secure-term",
                "OP-E Console & Secure-Term",
                "Designed and operate an end-to-end encrypted AI operations console and browser terminal. TypeScript/React front end, Python services, AES-GCM/WebCrypto, SSH into persistent tmux sessions, cost analytics, monitoring, recall, model routing and a supervised remote-desktop gateway, deployed behind Cloudflare Access with backups and recovery runbooks.",
                2026, "AI Systems Architect · Independent", null,
                new[] { "TypeScript", "React", "Python", "AES-GCM", "WebCrypto", "SSH / tmux", "Docker", "Cloudflare Access", "LLMs" }),
            new("career-automation-stack",
                "Career Automation Stack",
                "Self-hosted job and contract pipeline built on n8n, NocoDB/PostgreSQL and FastAPI. Scores roles against an evidence-based skill inventory, generates ATS-safe resumes, and runs the work as background jobs, with a human in control of every application.",
                2026, "Automation Architect · Independent", "/projects/jobdashboard",
                new[] { "n8n", "NocoDB", "PostgreSQL", "FastAPI", "Python", "LLM", "Docker" }),
            new("agent-test-command-deck",
                "Agent-Test & Command Deck",
                "Disposable-container CI harness for static, unit/API/UI and hardcoded-value checks, plus a config-driven Docker control plane with health probes, guarded actions and Cloudflare Access. Built so agent-generated and human changes pass the same verification gate.",
                2026, "Platform & Verification Architect · Independent", null,
                new[] { "Docker", "pytest", "Browserless", "FastAPI", "Docker SDK", "Cloudflare Access", "CI/CD" }),
            new("zero-trust-platform",
                "Zero Trust Self-Hosted Platform",
                "Self-hosted Linux and Docker platform published only through Cloudflare Zero Trust tunnels, with identity-aware access, monitoring, alerting, backups and disaster-recovery runbooks. Designed to fail visibly and recover without manual babysitting.",
                2026, "Platform Architect · Independent", null,
                new[] { "Linux", "Docker", "Cloudflare Zero Trust", "Monitoring", "Backups / DR", "Networking" }),
            new("local-ai-deliberation",
                "Local AI Deliberation Council",
                "Multi-agent deliberation system where local models answer independently, cross-examine blind-labelled rounds and a separate judge reads the verdict. Runs entirely on owned hardware via Ollama, with structured outputs and no data leaving the network.",
                2026, "AI Systems Architect · Independent", "/projects/ouroborn",
                new[] { "Python", "Ollama", "LLM", "Multi-agent systems", "FastAPI", "SQLite" }),
        };

        private static string Q(string s) => "'" + s.Replace("'", "''") + "'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var r in Rows)
            {
                var link = r.Link is null ? "NULL" : Q(r.Link);
                migrationBuilder.Sql($"""
                    INSERT INTO "Projects" ("Id", "Title", "Blurb", "Year", "Role", "Link", "Repo", "Tech")
                    VALUES ({Q(r.Id)}, {Q(r.Title)}, {Q(r.Blurb)}, {r.Year}, {Q(r.Role)}, {link}, NULL, {Q(JsonSerializer.Serialize(r.Tech))})
                    ON CONFLICT ("Id") DO UPDATE SET
                        "Title" = excluded."Title",
                        "Blurb" = excluded."Blurb",
                        "Year"  = excluded."Year",
                        "Role"  = excluded."Role",
                        "Link"  = excluded."Link",
                        "Tech"  = excluded."Tech";
                    """);
            }

            var ids = string.Join(", ", Rows.Select(r => Q(r.Id)));
            migrationBuilder.Sql($"""DELETE FROM "Projects" WHERE "Id" NOT IN ({ids});""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var ids = string.Join(", ", Rows.Select(r => Q(r.Id)));
            migrationBuilder.Sql($"""DELETE FROM "Projects" WHERE "Id" IN ({ids});""");
        }
    }
}
