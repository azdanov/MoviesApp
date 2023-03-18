using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

internal class RatingService : IRatingService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<RateMovieOptions> _rateMovieOptionsValidator;
    private readonly IRatingRepository _ratingRepository;

    public RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository,
        IValidator<RateMovieOptions> rateMovieOptionsValidator)
    {
        _ratingRepository = ratingRepository;
        _movieRepository = movieRepository;
        _rateMovieOptionsValidator = rateMovieOptionsValidator;
    }

    public async Task<bool> RateMovieAsync(RateMovieOptions options, CancellationToken token = default)
    {
        await _rateMovieOptionsValidator.ValidateAndThrowAsync(options, token);

        var movieExists = await _movieRepository.ExistsByIdAsync(options.MovieId, token);

        if (!movieExists) return false;

        return await _ratingRepository.RateMovieAsync(options, token);
    }

    public Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        return _ratingRepository.DeleteRatingAsync(movieId, userId, token);
    }

    public Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken token = default)
    {
        return _ratingRepository.GetRatingsForUserAsync(userId, token);
    }
}