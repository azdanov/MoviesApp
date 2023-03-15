using Movies.Application.Services;

namespace Movies.Application.Models;

public class Movie
{
    public required Guid Id { get; init; }

    public string Slug => GenerateSlug();

    public required string Title { get; set; }

    public required int YearOfRelease { get; set; }

    public required List<string> Genres { get; init; } = new();

    private string GenerateSlug()
    {
        return SlugService.GenerateSlug($"{Title} {YearOfRelease}");
    }
}