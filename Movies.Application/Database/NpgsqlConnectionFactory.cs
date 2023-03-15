using System.Data.Common;
using Npgsql;

namespace Movies.Application.Database;

internal class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbDataSource CreateDataSource()
    {
        return NpgsqlDataSource.Create(_connectionString);
    }
}