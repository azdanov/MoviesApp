namespace Movies.Contracts.Responses;

public class MovieRatingsResponse
{
    public required IEnumerable<MovieRatingResponse> Items { get; init; } = Enumerable.Empty<MovieRatingResponse>();
}