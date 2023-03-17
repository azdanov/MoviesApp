using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

internal class PostgreSqlMovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public PostgreSqlMovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO movies (id, slug, title, year_of_release)
            VALUES (@Id, @Slug, @Title, @YearOfRelease);
            """, movie,
            transaction, cancellationToken: cancellationToken));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                result += await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre },
                    transaction, cancellationToken: cancellationToken));
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
            SELECT m.id, m.slug, m.title, m.year_of_release, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS user_rating
            FROM movies m
                     LEFT JOIN ratings r ON r.movie_id = m.id
                     LEFT JOIN ratings myr ON myr.movie_id = m.id AND myr.user_id = @UserId
            WHERE m.id = @Id
            GROUP BY m.id, user_rating;
            """, new { Id = movieId, UserId = userId },
            transaction, cancellationToken: cancellationToken));

        if (movie is not null)
        {
            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = movieId },
                transaction, cancellationToken: cancellationToken));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
            SELECT m.id, m.slug, m.title, m.year_of_release, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS user_rating
            FROM movies m
                     LEFT JOIN ratings r ON r.movie_id = m.id
                     LEFT JOIN ratings myr ON myr.movie_id = m.id AND myr.user_id = @UserId
            WHERE m.slug = @Slug
            GROUP BY m.id, user_rating;
            """, new { Slug = slug, UserId = userId },
            transaction, cancellationToken: cancellationToken));

        if (movie is not null)
        {
            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = movie.Id },
                transaction, cancellationToken: cancellationToken));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var results = await connection.QueryAsync(new CommandDefinition("""
            SELECT m.id,
                   m.slug,
                   m.title,
                   m.year_of_release,
                   STRING_AGG(DISTINCT g.name, ',') AS genres,
                   ROUND(AVG(r.rating), 1)             AS rating,
                   myr.rating                       AS user_rating
            FROM movies m
                     LEFT JOIN genres g ON m.id = g.movie_id
                     LEFT JOIN ratings r ON r.movie_id = m.id
                     LEFT JOIN ratings myr ON myr.movie_id = m.id AND myr.user_id = @UserId
            GROUP BY m.id, user_rating;
            """, new { UserId = userId },
            transaction, cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);

        return results.Select(result => new Movie
        {
            Id = result.id,
            Title = result.title,
            YearOfRelease = result.year_of_release,
            Rating = (float?)result.rating,
            UserRating = (int?)result.user_rating,
            Genres = Enumerable.ToList(result.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE movies
            SET slug            = @Slug,
                title           = @Title,
                year_of_release = @YearOfRelease
            WHERE id = @Id;
            """,
            movie, transaction, cancellationToken: cancellationToken));

        if (result > 0)
        {
            result += await connection.ExecuteAsync(new CommandDefinition("""
                DELETE
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = movie.Id },
                transaction, cancellationToken: cancellationToken));

            foreach (var genre in movie.Genres)
            {
                result += await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre },
                    transaction, cancellationToken: cancellationToken));
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            DELETE
            FROM genres
            WHERE movie_id = @MovieId;
            """, new { MovieId = movieId },
            transaction, cancellationToken: cancellationToken));

        if (result > 0)
        {
            result += await connection.ExecuteAsync("""
                DELETE
                FROM movies
                WHERE id = @Id;
                """, new { Id = movieId });
        }

        await transaction.CommitAsync(cancellationToken);

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            SELECT COUNT(*)
            FROM movies
            WHERE id = @Id;
            """, new { Id = movieId },
            cancellationToken: cancellationToken));

        return result > 0;
    }
}