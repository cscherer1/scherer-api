using System.Text.RegularExpressions;

namespace Scherer.Api.Features.Projects.Services;

public static class SlugGenerator
{
    private static readonly Regex NonWord = new("[^a-z0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string From(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "item";
        var slug = input.Trim().ToLowerInvariant();
        slug = NonWord.Replace(slug, "-");
        slug = slug.Trim('-');
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }
}
