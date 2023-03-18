namespace Movies.Application.Models;

public class RateMovieOptions
{
    public Guid MovieId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }
}