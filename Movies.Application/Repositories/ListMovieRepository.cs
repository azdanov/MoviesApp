using Movies.Application.Models;

namespace Movies.Application.Repositories;

internal class ListMovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = new();

    public Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _movies.Add(movie);
        return Task.FromResult(true);
    }

    public Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        var movie = _movies.SingleOrDefault(m => m.Id == movieId);
        return Task.FromResult(movie);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        var movie = _movies.SingleOrDefault(m => m.Slug == slug);
        return Task.FromResult(movie);
    }

    public Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_movies.AsEnumerable());
    }

    public Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        var index = _movies.FindIndex(m => m.Id == movie.Id);
        if (index == -1) return Task.FromResult(false);

        _movies[index] = movie;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var index = _movies.FindIndex(m => m.Id == movieId);
        if (index == -1) return Task.FromResult(false);

        _movies.RemoveAt(index);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var exists = _movies.Any(m => m.Id == movieId);
        return Task.FromResult(exists);
    }
}