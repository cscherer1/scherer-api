namespace Scherer.Api.Features.Projects.Dtos;

using Scherer.Api.Features.Projects.Models;

public record ProjectsDto(
    string Heading,
    List<Project> Items
);
