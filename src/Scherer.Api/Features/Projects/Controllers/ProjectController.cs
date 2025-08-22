using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<Project>> Create([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        // minimal validation (add FluentValidation later if you want)
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");
        if (req.Year < 1990 || req.Year > DateTime.UtcNow.Year + 1)
            return BadRequest("Year looks invalid.");

        // coalesce tech safely (no empty catch / no mutation of init-only DTO)
        var tech = req.Tech ?? new List<string>();

        // generate a unique, URL-safe id from the title
        var baseId = SlugGenerator.From(req.Title);
        var id = baseId;
        var n = 2;
        while (await repo.GetByIdAsync(id, ct) is not null)
        {
            id = $"{baseId}-{n++}";
        }

        var project = new Project(
            Id: id,
            Title: req.Title,
            Blurb: req.Blurb,
            Tech: tech,
            Year: req.Year,
            Role: req.Role,
            Link: req.Link,
            Repo: req.Repo
        );

        await repo.AddAsync(project, ct);

        // 201 Created + Location header to GET /api/projects/{id}
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

}
