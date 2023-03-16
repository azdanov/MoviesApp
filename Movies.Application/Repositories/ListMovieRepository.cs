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

    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var movie = _movies.SingleOrDefault(m => m.Id == id);
        return Task.FromResult(movie);
    }

    public Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
    {
        var movie = _movies.SingleOrDefault(m => m.Slug == slug);
        return Task.FromResult(movie);
    }

    public Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
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

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        var index = _movies.FindIndex(m => m.Id == id);
        if (index == -1) return Task.FromResult(false);

        _movies.RemoveAt(index);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        var exists = _movies.Any(m => m.Id == id);
        return Task.FromResult(exists);
    }
}