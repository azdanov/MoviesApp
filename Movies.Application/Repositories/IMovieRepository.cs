using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default, CancellationToken cancellationToken = default);

    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default);

    Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid movieId, CancellationToken cancellationToken = default);
}