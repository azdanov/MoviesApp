using System.Data;
using Dapper;

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
}