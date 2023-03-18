using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie, CancellationToken token = default);

    Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default, CancellationToken token = default);

    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default);

    Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default);

    Task<bool> UpdateAsync(Movie movie, CancellationToken token = default);

    Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken token = default);

    Task<bool> ExistsByIdAsync(Guid movieId, CancellationToken token = default);

    Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default);
}