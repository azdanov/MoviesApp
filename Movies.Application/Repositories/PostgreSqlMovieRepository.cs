using Dapper;
using Movies.Application.Database;
using Movies.Application.Extensions;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

internal class PostgreSqlMovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public PostgreSqlMovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    // There should be many-to-many relationship between movies and genres.
    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO movies (id, slug, title, year_of_release)
            VALUES (@Id, @Slug, @Title, @YearOfRelease);
            """, movie,
            transaction, cancellationToken: token));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                result += await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre },
                    transaction, cancellationToken: token));
            }
        }

        await transaction.CommitAsync(token);

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId = default, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
            SELECT m.id, m.slug, m.title, m.year_of_release, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS user_rating
            FROM movies m
                     LEFT JOIN ratings r ON r.movie_id = m.id
                     LEFT JOIN ratings myr ON myr.movie_id = m.id AND myr.user_id = @UserId
            WHERE m.id = @Id
            GROUP BY m.id, user_rating;
            """, new { Id = movieId, UserId = userId },
            transaction, cancellationToken: token));

        if (movie is not null)
        {
            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = movieId },
                transaction, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync(token);

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
            SELECT m.id, m.slug, m.title, m.year_of_release, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS user_rating
            FROM movies m
                     LEFT JOIN ratings r ON r.movie_id = m.id
                     LEFT JOIN ratings myr ON myr.movie_id = m.id AND myr.user_id = @UserId
            WHERE m.slug = @Slug
            GROUP BY m.id, user_rating;
            """, new { Slug = slug, UserId = userId },
            transaction, cancellationToken: token));

        if (movie is not null)
        {
            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = movie.Id },
                transaction, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync(token);

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var orderClause = string.Empty;
        if (options.SortField is not null)
        {
            var sortField = options.SortField.ToSnakeCase();
            orderClause = options.SortOrder switch
            {
                SortOrder.Ascending => $"ORDER BY {sortField} ASC",
                SortOrder.Descending => $"ORDER BY {sortField} DESC",
                _ => string.Empty
            };
        }

        var results = await connection.QueryAsync(new CommandDefinition($"""
            SELECT m.id                             AS id,
                   m.slug                           AS slug,
                   m.title                          AS title,
                   m.year_of_release                AS year_of_release,
                   STRING_AGG(DISTINCT g.name, ',') AS genres,
                   ROUND(AVG(r.rating), 1)          AS rating,
                   myr.rating                       AS user_rating
            FROM movies m
                     LEFT JOIN genres g ON m.id = g.movie_id
                     LEFT JOIN ratings r ON r.movie_id = m.id
                     LEFT JOIN ratings myr ON myr.movie_id = m.id AND myr.user_id = @UserId
            WHERE (@Title IS NULL OR m.title ILIKE ('%' || @Title || '%'))
              AND (@YearOfRelease IS NULL OR m.year_of_release = @YearOfRelease)
            GROUP BY m.id, user_rating
            {orderClause};
            """, new { options.UserId, options.Title, options.YearOfRelease },
            transaction, cancellationToken: token));

        await transaction.CommitAsync(token);

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

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE movies
            SET slug            = @Slug,
                title           = @Title,
                year_of_release = @YearOfRelease
            WHERE id = @Id;
            """,
            movie, transaction, cancellationToken: token));

        if (result > 0)
        {
            result += await connection.ExecuteAsync(new CommandDefinition("""
                DELETE
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = movie.Id },
                transaction, cancellationToken: token));

            foreach (var genre in movie.Genres)
            {
                result += await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre },
                    transaction, cancellationToken: token));
            }
        }

        await transaction.CommitAsync(token);

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid movieId, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            DELETE
            FROM genres
            WHERE movie_id = @MovieId;
            """, new { MovieId = movieId },
            transaction, cancellationToken: token));

        if (result > 0)
        {
            result += await connection.ExecuteAsync("""
                DELETE
                FROM movies
                WHERE id = @Id;
                """, new { Id = movieId });
        }

        await transaction.CommitAsync(token);

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid movieId, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            SELECT COUNT(*)
            FROM movies
            WHERE id = @Id;
            """, new { Id = movieId },
            cancellationToken: token));

        return result > 0;
    }
}