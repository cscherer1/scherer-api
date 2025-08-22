using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Scherer.Api.Data;

public static class DbInitializer
{
    public static async Task MigrateAndSeedAsync(AppDbContext db, ILogger logger, bool isDevelopment, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!isDevelopment) return;               // seed only in Development
        if (await db.Projects.AnyAsync(ct)) return;

        logger.LogInformation("Seeding Projects (dev only)...");
        db.Projects.AddRange(
            new ProjectEntity
            {
                Id = "adobe-licensing",
                Title = "Adobe License Automation",
                Blurb = "Automated license lifecycle to cut costs & admin toil.",
                Year = 2025,
                Role = "Lead Developer",
                Link = null,
                Repo = null,
                TechJson = JsonSerializer.Serialize(new[] { "C#", "Azure Functions", "PowerShell" })
            },
            new ProjectEntity
            {
                Id = "server-patching",
                Title = "Monthly Server Patching Orchestrator",
                Blurb = "Config-driven scheduling engine with RITM/SCTASK generation, approvals, and status rollups.",
                Year = 2025,
                Role = "Solutions Architect",
                Link = null,
                Repo = null,
                TechJson = JsonSerializer.Serialize(new[] { "ServiceNow", "Workflow", "PowerShell" })
            },
            new ProjectEntity
            {
                Id = "dfs-archive",
                Title = "DFS Archive Request Pipeline",
                Blurb = "MID Server + PowerShell pipeline with guarded concurrency and throttling for bulk archive jobs.",
                Year = 2025,
                Role = "Lead Developer",
                Link = null,
                Repo = null,
                TechJson = JsonSerializer.Serialize(new[] { "MID Server", "PowerShell", "Governance" })
            }
        );
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeding complete.");
    }
}
