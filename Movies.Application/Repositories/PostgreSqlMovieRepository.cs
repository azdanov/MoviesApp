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

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
            SELECT id, slug, title, year_of_release
            FROM movies
            WHERE id = @Id;
            """, new { Id = id },
            transaction, cancellationToken: token));

        if (movie is not null)
        {
            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;
                """, new { MovieId = id },
                transaction, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync(token);

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
            SELECT id, slug, title, year_of_release
            FROM movies
            WHERE slug = @Slug;
            """, new { Slug = slug },
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

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var results = await connection.QueryAsync(new CommandDefinition("""
            SELECT m.id, m.slug, m.title, m.year_of_release, string_agg(g.name, ',') as genres
            FROM movies m
                LEFT JOIN genres g ON m.id = g.movie_id
            GROUP BY m.id;
            """,
            transaction, cancellationToken: token));

        await transaction.CommitAsync(token);

        return results.Select(result => new Movie
        {
            Id = result.id,
            Title = result.title,
            YearOfRelease = result.year_of_release,
            Genres = Enumerable.ToList(result.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE movies
            SET slug = @Slug, title = @Title, year_of_release = @YearOfRelease
            WHERE id = @Id;
            """,
            movie, transaction, cancellationToken: token));

        if (result > 0)
        {
            result += await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres
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

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync(token);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            DELETE FROM genres
            WHERE movie_id = @MovieId;
            """, new { MovieId = id },
            transaction, cancellationToken: token));

        if (result > 0)
        {
            result += await connection.ExecuteAsync("""
                DELETE FROM movies
                WHERE id = @Id;
                """, new { Id = id });
        }

        await transaction.CommitAsync(token);

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            SELECT COUNT(*)
            FROM movies
            WHERE id = @Id;
            """, new { Id = id },
            cancellationToken: token));

        return result > 0;
    }
}