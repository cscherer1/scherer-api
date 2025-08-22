namespace Scherer.Api.Features.Projects.Dtos;

public record UpdateProjectRequest(
    string Title,
    string Blurb,
    List<string>? Tech,
    int Year,
    string Role,
    string? Link = null,
    string? Repo = null
);
