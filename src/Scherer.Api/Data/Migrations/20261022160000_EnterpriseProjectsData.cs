using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scherer.Api.Data.Migrations
{
    /// <summary>
    /// Data-only migration: upserts the enterprise-positioning projects.
    /// Runs automatically on startup (Program.cs -> MigrateAsync), so Render applies it to Neon on next deploy.
    /// Existing projects are untouched; no schema change, so the model snapshot is unchanged.
    /// INSERT ... ON CONFLICT is valid on both PostgreSQL (prod) and SQLite (dev).
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261022160000_EnterpriseProjectsData")]
    public partial class EnterpriseProjectsData : Migration
    {
        private sealed record Row(string Id, string Title, string Blurb, int Year, string Role, string[] Tech);

        private static readonly Row[] Rows =
        {
            new("zero-trust-homelab-infrastructure",
                "Zero Trust Homelab Infrastructure",
                "Designed and operates a 20+ service containerized homelab on Ubuntu Linux, exposed exclusively through Cloudflare Zero Trust tunnels with no open inbound ports. Stack includes AdGuard Home for split-horizon DNS, Cockpit for web-based system management, Portainer for container orchestration, and automated ClamAV scanning pipelines.",
                2026, "Systems Engineer / Architect",
                new[] { "Docker", "Linux", "Cloudflare Zero Trust", "AdGuard Home" }),
            new("ibm-i-azure-integration-bridge",
                "IBM i ↔ Azure Integration Bridge",
                "Architects and maintains integration bridges connecting IBM i (AS/400) business logic written in RPG/CL to modern cloud services including Azure, Python REST APIs, and .NET endpoints. Eliminates the legacy/modern divide for enterprise ERP data flows.",
                2026, "Programmer/Analyst / Integration Engineer",
                new[] { "IBM i", "RPG", "CL", "Azure", "Python", ".NET" }),
            new("m365-tenant-automation-governance",
                "M365 Tenant Automation & Governance",
                "Engineered enterprise-scale PowerShell automation at Burns & McDonnell operating one tier below Global Administrator. Covered user/identity lifecycle provisioning, Intune MDM/MAM policy enforcement, Entra ID RBAC drift correction, Exchange Online mailbox administration, and OneDrive for Business compliance.",
                2025, "M365 Sub-Global Administrator / Automation Engineer",
                new[] { "PowerShell", "M365", "Intune", "Entra ID", "Exchange Online", "Azure AD" }),
            new("servicenow-enterprise-workflow-platform",
                "ServiceNow Enterprise Workflow Platform",
                "Built end-to-end workflow automations at Burns & McDonnell including Business Rules, UI Actions, Script Includes, and custom Service Catalog integrations via REST APIs. Reduced manual IT process overhead across cross-departmental service delivery.",
                2025, "Lead Developer / Solutions Architect",
                new[] { "ServiceNow", "JavaScript", "REST APIs" }),
            new("local-llm-ai-agent-orchestration-stack",
                "Local LLM & AI Agent Orchestration Stack",
                "Runs local open-weight LLMs via Ollama for privacy-preserving AI inference. Integrates agent frameworks with local file systems, bash runtimes, and system APIs. Implements contextual prompt chaining and structured tool-calling pipelines connecting model outputs to operational OS commands.",
                2026, "AI Systems Engineer",
                new[] { "Ollama", "Python", "Docker", "Linux", "LLM" }),
        };

        private static string Q(string s) => "'" + s.Replace("'", "''") + "'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var r in Rows)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO "Projects" ("Id", "Title", "Blurb", "Year", "Role", "Link", "Repo", "Tech")
                    VALUES ({Q(r.Id)}, {Q(r.Title)}, {Q(r.Blurb)}, {r.Year}, {Q(r.Role)}, NULL, NULL, {Q(JsonSerializer.Serialize(r.Tech))})
                    ON CONFLICT ("Id") DO UPDATE SET
                        "Title" = excluded."Title",
                        "Blurb" = excluded."Blurb",
                        "Year"  = excluded."Year",
                        "Role"  = excluded."Role",
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
