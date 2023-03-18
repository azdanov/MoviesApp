using Movies.Application.Models;

namespace Movies.Application.Repositories;

internal class ListMovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = new();

    public Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        _movies.Add(movie);
        return Task.FromResult(true);
    }

    public Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default,
        CancellationToken token = default)
    {
        var movie = _movies.SingleOrDefault(m => m.Id == movieId);
        return Task.FromResult(movie);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default,
        CancellationToken token = default)
    {
        var movie = _movies.SingleOrDefault(m => m.Slug == slug);
        return Task.FromResult(movie);
    }

    public Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        return Task.FromResult(_movies.AsEnumerable());
    }

    public Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        var index = _movies.FindIndex(m => m.Id == movie.Id);
        if (index == -1) return Task.FromResult(false);

        _movies[index] = movie;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken token = default)
    {
        var index = _movies.FindIndex(m => m.Id == movieId);
        if (index == -1) return Task.FromResult(false);

        _movies.RemoveAt(index);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsByIdAsync(Guid movieId, CancellationToken token = default)
    {
        var exists = _movies.Any(m => m.Id == movieId);
        return Task.FromResult(exists);
    }

    public Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
    {
        var count = _movies.Count(m =>
            (title is null || m.Title.Contains(title, StringComparison.OrdinalIgnoreCase)) &&
            (yearOfRelease is null || m.YearOfRelease == yearOfRelease));
        return Task.FromResult(count);
    }
}