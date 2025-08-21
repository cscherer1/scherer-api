using Microsoft.AspNetCore.Mvc;

namespace Scherer.Api.Features.Projects;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    public ActionResult<ProjectsDto> Get()
    {
        var dto = new ProjectsDto(
            Heading: "Projects",
            Items: new List<Project>
            {
                new(
                    Id: "adobe-licensing",
                    Title: "Adobe License Automation",
                    Blurb: "Policy-driven license sync across ServiceNow with scheduled reconciliation and exception handling.",
                    Tech: new List<string>{ "ServiceNow", "REST", "Automation" },
                    Year: 2024,
                    Role: "Solutions Architect"
                ),
                new(
                    Id: "server-patching",
                    Title: "Monthly Server Patching Orchestrator",
                    Blurb: "Config-driven scheduling engine with RITM/SCTASK generation, approvals, and status rollups.",
                    Tech: new List<string>{ "ServiceNow", "Workflow", "PowerShell" },
                    Year: 2023,
                    Role: "Lead Developer"
                ),
                new(
                    Id: "dfs-archive",
                    Title: "DFS Archive Request Pipeline",
                    Blurb: "MID Server + PowerShell pipeline with guarded concurrency and throttling for bulk archive jobs.",
                    Tech: new List<string>{ "MID Server", "PowerShell", "Governance" },
                    Year: 2023,
                    Role: "Developer"
                )
            }
        );

        return Ok(dto);
    }
}
