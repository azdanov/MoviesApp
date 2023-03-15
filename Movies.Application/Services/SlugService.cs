using Slugify;

namespace Movies.Application.Services;

public static class SlugService
{
    private static readonly SlugHelper _helper = new();

    public static string GenerateSlug(string title)
    {
        return _helper.GenerateSlug(title);
    }
}