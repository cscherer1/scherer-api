namespace Scherer.Api.Features.Projects.Repositories;

using Scherer.Api.Features.Projects.Models;

public interface IProjectsRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Project> AddAsync(Project project, CancellationToken ct = default);
    Task<Project?> UpdateAsync(string id, Project updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
