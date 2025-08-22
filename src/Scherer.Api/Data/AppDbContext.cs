using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Scherer.Api.Features.Projects.Models;

namespace Scherer.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var p = modelBuilder.Entity<ProjectEntity>();
        p.ToTable("Projects");

        p.HasKey(x => x.Id);
        p.Property(x => x.Id).HasMaxLength(128);

        p.Property(x => x.Title).IsRequired().HasMaxLength(120);
        p.Property(x => x.Blurb).IsRequired().HasMaxLength(600);
        p.Property(x => x.Role).IsRequired().HasMaxLength(80);

        // Store Tech as JSON in a TEXT column
        p.Property(x => x.TechJson)
            .HasColumnName("Tech")
            .IsRequired();

        p.Property(x => x.Year).IsRequired();
        p.Property(x => x.Link);
        p.Property(x => x.Repo);
    }
}

/// <summary>
/// EF Core storage model for Projects. Tech is stored as JSON (TechJson).
/// </summary>
public class ProjectEntity
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Blurb { get; set; } = default!;
    public int Year { get; set; }
    public string Role { get; set; } = default!;
    public string? Link { get; set; }
    public string? Repo { get; set; }

    // JSON backing column
    public string TechJson { get; set; } = "[]";

    // --- Mapping helpers ---

    public static ProjectEntity FromDomain(Project p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Blurb = p.Blurb,
        Year = p.Year,
        Role = p.Role,
        Link = p.Link,
        Repo = p.Repo,
        TechJson = JsonSerializer.Serialize(p.Tech ?? new List<string>()),
    };

    public Project ToDomain()
    {
        var tech = JsonSerializer.Deserialize<List<string>>(TechJson) ?? new List<string>();
        return new Project(Id, Title, Blurb, tech, Year, Role, Link, Repo);
    }
}
