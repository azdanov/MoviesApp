namespace Movies.Contracts.Responses;

public class MoviesResponse
{
    public required IEnumerable<MovieResponse> Items { get; init; } = Enumerable.Empty<MovieResponse>();
}