using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

internal class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> RateMovieAsync(RateMovieOptions options, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ratings (movie_id, user_id, rating)
            VALUES (@MovieId, @UserId, @Rating)
            ON CONFLICT (movie_id, user_id) DO UPDATE SET rating = @Rating;
            """, new { options.MovieId, options.UserId, options.Rating },
            cancellationToken: token));

        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
            SELECT ROUND(AVG(r.rating), 1) AS rating
            FROM ratings r
            WHERE r.movie_id = @MovieId;
            """, new { MovieId = movieId },
            cancellationToken: token));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId,
        CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<(float? Rating, int? UserRating)>(new CommandDefinition("""
            SELECT ROUND(AVG(r.rating), 1) AS rating,
                   (SELECT myr.rating
                    FROM ratings myr
                    WHERE myr.movie_id = @MovieId
                      AND myr.user_id = @UserId
                    LIMIT 1)               AS user_rating
            FROM ratings r
            WHERE r.movie_id = @MovieId;
            """, new { MovieId = movieId, UserId = userId },
            cancellationToken: token));
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            DELETE
            FROM ratings
            WHERE movie_id = @MovieId
              AND user_id = @UserId;
            """, new { MovieId = movieId, UserId = userId },
            cancellationToken: token));

        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QueryAsync<MovieRating>(new CommandDefinition("""
            SELECT r.movie_id AS movie_id,
                   m.slug     AS slug,
                   r.rating   AS rating
            FROM ratings r
                     INNER JOIN movies m ON m.id = r.movie_id
            WHERE r.user_id = @UserId;
            """, new { UserId = userId },
            cancellationToken: token));
    }
}