using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }

    public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
    {
        return new Movie
        {
            Id = id,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }

    public static MovieResponse MapToResponse(this Movie movie)
    {
        return new MovieResponse
        {
            Id = movie.Id,
            Slug = movie.Slug,
            Title = movie.Title,
            YearOfRelease = movie.YearOfRelease,
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            Genres = movie.Genres
        };
    }

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies)
    {
        return new MoviesResponse
        {
            Items = movies.Select(MapToResponse)
        };
    }

    public static MovieRatingResponse MapToResponse(this MovieRating movieRating)
    {
        return new MovieRatingResponse
        {
            MovieId = movieRating.MovieId,
            Slug = movieRating.Slug,
            Rating = movieRating.Rating
        };
    }

    public static MovieRatingsResponse MapToResponse(this IEnumerable<MovieRating> movieRatings)
    {
        return new MovieRatingsResponse
        {
            Items = movieRatings.Select(MapToResponse)
        };
    }

    public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
    {
        return new GetAllMoviesOptions
        {
            Title = request.Title,
            YearOfRelease = request.Year
        };
    }

    public static GetAllMoviesOptions WithUserId(this GetAllMoviesOptions options, Guid? userId)
    {
        options.UserId = userId;
        return options;
    }

    public static RateMovieOptions MapToOptions(this RateMovieRequest request)
    {
        return new RateMovieOptions
        {
            Rating = request.Rating
        };
    }

    public static RateMovieOptions WithMovieId(this RateMovieOptions options, Guid movieId)
    {
        options.MovieId = movieId;
        return options;
    }

    public static RateMovieOptions WithUserId(this RateMovieOptions options, Guid userId)
    {
        options.UserId = userId;
        return options;
    }
}