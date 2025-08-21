namespace Scherer.Api.Features.Projects.Models;

public record Project(
    string Id,
    string Title,
    string Blurb,
    List<string> Tech,
    int Year,
    string Role,
    string? Link = null,
    string? Repo = null
);
