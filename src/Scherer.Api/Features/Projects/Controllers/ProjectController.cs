using Microsoft.AspNetCore.Mvc;
using Scherer.Api.Features.Projects.Dtos;
using Scherer.Api.Features.Projects.Repositories;
using Scherer.Api.Features.Projects.Models;
using Scherer.Api.Features.Projects.Services;

namespace Scherer.Api.Features.Projects.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectsRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProjectsDto>> Get(CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        var dto = new ProjectsDto(
            Heading: "Projects",
            Items: items.ToList()
        );
        return Ok(dto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetById(string id, CancellationToken ct)
    {
        var found = await repo.GetByIdAsync(id, ct);
        if (found is null) return NotFound();
        return Ok(found);
    }

}
