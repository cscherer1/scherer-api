namespace Scherer.Api.Features.Projects.Repositories;

using System.Linq;
using Scherer.Api.Features.Projects.Models;

public class InMemoryProjectsRepository : IProjectsRepository
{
    private readonly List<Project> _items;
    private readonly object _lock = new();

    public InMemoryProjectsRepository()
    {
        _items = new List<Project>
        {
            new Project(
                Id: "adobe-licensing",
                Title: "Adobe License Automation",
                Blurb: "Policy-driven license sync across ServiceNow with scheduled reconciliation and exception handling.",
                Tech: new List<string>{ "ServiceNow", "REST", "Automation" },
                Year: 2024,
                Role: "Solutions Architect"
            ),
            new Project(
                Id: "server-patching",
                Title: "Monthly Server Patching Orchestrator",
                Blurb: "Config-driven scheduling engine with RITM/SCTASK generation, approvals, and status rollups.",
                Tech: new List<string>{ "ServiceNow", "Workflow", "PowerShell" },
                Year: 2023,
                Role: "Lead Developer"
            ),
            new Project(
                Id: "dfs-archive",
                Title: "DFS Archive Request Pipeline",
                Blurb: "MID Server + PowerShell pipeline with guarded concurrency and throttling for bulk archive jobs.",
                Tech: new List<string>{ "MID Server", "PowerShell", "Governance" },
                Year: 2023,
                Role: "Developer"
            )
        };
    }

    public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            // return a copy to avoid external mutation
            return Task.FromResult((IReadOnlyList<Project>)_items.ToList());
        }
    }

    public Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _items.Add(project);
            return Task.FromResult(project);
        }
    }

    public Task<Project?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var match = _items.FirstOrDefault(p =>
                p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(match);
        }
    }

}
