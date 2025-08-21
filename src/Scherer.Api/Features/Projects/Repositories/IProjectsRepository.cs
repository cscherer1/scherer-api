namespace Scherer.Api.Features.Projects.Repositories;

using Scherer.Api.Features.Projects.Models;

public interface IProjectsRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    // Prep for later POST
    Task<Project> AddAsync(Project project, CancellationToken ct = default);
}
