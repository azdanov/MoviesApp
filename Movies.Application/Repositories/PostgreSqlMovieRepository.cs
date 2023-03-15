using Dapper;
using Dapper.Transaction;
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

    public async Task<bool> CreateAsync(Movie movie)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var result = await transaction.ExecuteAsync(@"
            INSERT INTO movies (id, slug, title, year_of_release)
            VALUES (@Id, @Slug, @Title, @YearOfRelease);", movie);

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                result += await transaction.ExecuteAsync(@"
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre });
            }
        }

        await transaction.CommitAsync();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var movie = await transaction.QueryFirstOrDefaultAsync<Movie>(@"
            SELECT id, slug, title, year_of_release
            FROM movies
            WHERE id = @Id;", new { Id = id });

        if (movie is not null)
        {
            var genres = await transaction.QueryAsync<string>(@"
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;", new { MovieId = id });

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync();

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var movie = await transaction.QueryFirstOrDefaultAsync<Movie>(@"
            SELECT id, slug, title, year_of_release
            FROM movies
            WHERE slug = @Slug;", new { Slug = slug });

        if (movie is not null)
        {
            var genres = await transaction.QueryAsync<string>(@"
                SELECT name
                FROM genres
                WHERE movie_id = @MovieId;", new { MovieId = movie.Id });

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }
        }

        await transaction.CommitAsync();

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var results = await transaction.QueryAsync(@"
            SELECT m.id, m.slug, m.title, m.year_of_release, string_agg(g.name, ',') as genres
            FROM movies m
                LEFT JOIN genres g ON m.id = g.movie_id
            GROUP BY m.id;");

        await transaction.CommitAsync();

        return results.Select(result => new Movie
        {
            Id = result.id,
            Title = result.title,
            YearOfRelease = result.year_of_release,
            Genres = Enumerable.ToList(result.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var result = await transaction.ExecuteAsync(@"
            UPDATE movies
            SET slug = @Slug, title = @Title, year_of_release = @YearOfRelease
            WHERE id = @Id;", movie);

        if (result > 0)
        {
            result += await transaction.ExecuteAsync(@"
                DELETE FROM genres
                WHERE movie_id = @MovieId;", new { MovieId = movie.Id });

            foreach (var genre in movie.Genres)
            {
                result += await transaction.ExecuteAsync(@"
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre });
            }
        }

        await transaction.CommitAsync();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var result = await transaction.ExecuteAsync(@"
            DELETE FROM genres
            WHERE movie_id = @MovieId;", new { MovieId = id });

        if (result > 0)
        {
            result += await transaction.ExecuteAsync(@"
                DELETE FROM movies
                WHERE id = @Id;", new { Id = id });
        }

        await transaction.CommitAsync();

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var result = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*)
            FROM movies
            WHERE id = @Id;", new { Id = id });

        return result > 0;
    }
}