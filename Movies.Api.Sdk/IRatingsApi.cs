using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Refit;

namespace Movies.Api.Sdk;

public interface IRatingsApi
{
    [Get(ApiEndpoints.Ratings.GetUserRatings)]
    Task<MovieRatingsResponse> GetUserRatingsAsync();

    [Put(ApiEndpoints.Movies.Rate)]
    Task RateMovieAsync(string movieId, RateMovieRequest request);

    [Delete(ApiEndpoints.Movies.DeleteRating)]
    Task DeleteRatingAsync(string movieId);
}