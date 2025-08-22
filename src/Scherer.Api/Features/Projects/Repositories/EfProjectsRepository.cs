using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Scherer.Api.Data;
using Scherer.Api.Features.Projects.Models;

namespace Scherer.Api.Features.Projects.Repositories;

public class EfProjectsRepository(AppDbContext db) : IProjectsRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await db.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.Year)
            .ToListAsync(ct);

        return rows.Select(e => e.ToDomain()).ToList();
    }

    public async Task<Project?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return row?.ToDomain();
    }

    public async Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        var row = ProjectEntity.FromDomain(project);
        db.Projects.Add(row);
        await db.SaveChangesAsync(ct);
        return row.ToDomain();
    }

    public async Task<Project?> UpdateAsync(string id, Project updated, CancellationToken ct = default)
    {
        var row = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (row is null) return null;

        row.Title = updated.Title;
        row.Blurb = updated.Blurb;
        row.Year = updated.Year;
        row.Role = updated.Role;
        row.Link = updated.Link;
        row.Repo = updated.Repo;
        row.TechJson = JsonSerializer.Serialize(updated.Tech ?? new List<string>());

        await db.SaveChangesAsync(ct);
        return row.ToDomain();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (row is null) return false;

        db.Projects.Remove(row);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
