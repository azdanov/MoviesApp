using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default, CancellationToken cancellationToken = default);

    Task<Movie?> GetBySlugAsync(string movieSlug, Guid? userId = default,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default);

    Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken cancellationToken = default);
}