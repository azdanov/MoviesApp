using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

internal class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IValidator<GetAllMoviesOptions> _getAllMoviesOptionsValidator;
    private readonly IRatingRepository _ratingRepository;


    public MovieService(IMovieRepository movieRepository, IRatingRepository ratingRepository,
        IValidator<Movie> movieValidator, IValidator<GetAllMoviesOptions> getAllMoviesOptionsValidator)
    {
        _movieRepository = movieRepository;
        _ratingRepository = ratingRepository;
        _movieValidator = movieValidator;
        _getAllMoviesOptionsValidator = getAllMoviesOptionsValidator;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, token);
        return await _movieRepository.CreateAsync(movie, token);
    }

    public Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default, CancellationToken token = default)
    {
        return _movieRepository.GetByIdAsync(movieId, userId, token);
    }

    public Task<Movie?> GetBySlugAsync(string movieSlug, Guid? userId = default, CancellationToken token = default)
    {
        return _movieRepository.GetBySlugAsync(movieSlug, userId, token);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        await _getAllMoviesOptionsValidator.ValidateAndThrowAsync(options, token);
        return await _movieRepository.GetAllAsync(options, token);
    }

    // Use Unit of work pattern to ensure that all changes are committed or rolled back
    // https://stackoverflow.com/a/60565419
    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, token);

        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, token);
        if (!movieExists) return null;

        var result = await _movieRepository.UpdateAsync(movie, token);
        if (!result) return null;

        if (userId == null)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, token);
            movie.Rating = rating;
        }
        else
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, token);
            movie.Rating = rating.Rating;
            movie.UserRating = rating.UserRating;
        }

        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken token = default)
    {
        return _movieRepository.DeleteByIdAsync(movieId, token);
    }
}