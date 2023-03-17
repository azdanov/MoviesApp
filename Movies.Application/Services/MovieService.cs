using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

internal class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IRatingRepository _ratingRepository;

    public MovieService(IMovieRepository movieRepository, IRatingRepository ratingRepository,
        IValidator<Movie> movieValidator)
    {
        _movieRepository = movieRepository;
        _ratingRepository = ratingRepository;
        _movieValidator = movieValidator;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }

    public Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetByIdAsync(movieId, userId, cancellationToken);
    }

    public Task<Movie?> GetBySlugAsync(string movieSlug, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetBySlugAsync(movieSlug, userId, cancellationToken);
    }

    public Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetAllAsync(userId, cancellationToken);
    }

    // Use Unit of work pattern to ensure that all changes are committed or rolled back
    // https://stackoverflow.com/a/60565419
    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);

        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists) return null;

        var result = await _movieRepository.UpdateAsync(movie, cancellationToken);
        if (!result) return null;

        if (userId == null)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
        }
        else
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
            movie.Rating = rating.Rating;
            movie.UserRating = rating.UserRating;
        }

        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return _movieRepository.DeleteByIdAsync(movieId, cancellationToken);
    }
}