namespace Scherer.Api.Features.Projects;

public record ProjectsDto(
    string Heading,
    List<Project> Items
);
