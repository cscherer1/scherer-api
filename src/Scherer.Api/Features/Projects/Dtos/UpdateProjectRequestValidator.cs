using FluentValidation;

namespace Scherer.Api.Features.Projects.Dtos;

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Blurb)
            .NotEmpty()
            .MaximumLength(600);

        RuleFor(x => x.Tech)
            .Must(t => t == null || (t.Count >= 0 && t.Count <= 12))
            .WithMessage("Tech can include up to 12 items.");

        RuleForEach(x => x.Tech!)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.Year)
            .InclusiveBetween(1990, DateTime.UtcNow.Year + 1);

        RuleFor(x => x.Role)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Link)
            .Must(BeValidHttpsUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Link))
            .WithMessage("Link must be a valid https:// URL.");

        RuleFor(x => x.Repo)
            .Must(BeValidHttpsUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Repo))
            .WithMessage("Repo must be a valid https:// URL.");
    }

    private static bool BeValidHttpsUrl(string? url)
        => string.IsNullOrWhiteSpace(url)
           || (Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps);
}
