using Slugify;

namespace Movies.Application.Services;

public static class SlugService
{
    private static readonly SlugHelper Helper = new();

    public static string GenerateSlug(string title)
    {
        return Helper.GenerateSlug(title);
    }
}