using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie, CancellationToken token = default);

    Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default, CancellationToken token = default);

    Task<Movie?> GetBySlugAsync(string movieSlug, Guid? userId = default,
        CancellationToken token = default);

    Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default);

    Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default);

    Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken token = default);
}