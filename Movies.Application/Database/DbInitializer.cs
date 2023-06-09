using System.Data;
using System.Text.Json;
using Dapper;
using Movies.Application.Models;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        var connection = await _dbConnectionFactory.CreateConnectionAsync();

        await CreateMovies(connection);
        await CreateGenres(connection);
        await CreateRatings(connection);

        await SeedMovies(connection);
    }

    private static async Task CreateMovies(IDbConnection connection)
    {
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS movies (
                id uuid PRIMARY KEY,
                slug text NOT NULL,
                title text NOT NULL,
                year_of_release integer NOT NULL,
                CONSTRAINT movies_slug_uq UNIQUE (slug) DEFERRABLE INITIALLY DEFERRED
            );
        ");
    }

    private static async Task CreateGenres(IDbConnection connection)
    {
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS genres (
                movie_id uuid NOT NULL REFERENCES movies(id) ON DELETE CASCADE,
                name text NOT NULL
            );
        ");
    }

    private static async Task CreateRatings(IDbConnection connection)
    {
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS ratings (
                user_id uuid NOT NULL,
                movie_id uuid NOT NULL REFERENCES movies(id) ON DELETE CASCADE,
                rating integer NOT NULL,
                PRIMARY KEY (user_id, movie_id)
            );
        ");
    }

    private static async Task SeedMovies(IDbConnection connection)
    {
        var moviesList = JsonSerializer.Deserialize<List<Movie>>(Seed.Movies);

        var moviesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM movies;");

        if (moviesCount > 0 || moviesList == null) return;

        foreach (var movie in moviesList)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO movies (id, slug, title, year_of_release)
                VALUES (@Id, @Slug, @Title, @YearOfRelease);
            ", movie);

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO genres (movie_id, name)
                    VALUES (@MovieId, @Name);
                ", new { MovieId = movie.Id, Name = genre });
            }
        }
    }
}