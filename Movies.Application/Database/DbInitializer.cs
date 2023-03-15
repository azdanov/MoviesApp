namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        await using var dataSource = _connectionFactory.CreateDataSource();
        await using var command = dataSource.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS movies (
                id uuid PRIMARY KEY,
                slug text NOT NULL,
                title text NOT NULL,
                year_of_release integer NOT NULL
            );
        ";
        await command.ExecuteNonQueryAsync();

        command.CommandText = @"
            CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_uq
                ON movies USING btree(slug);
        ";
        await command.ExecuteNonQueryAsync();
    }
}