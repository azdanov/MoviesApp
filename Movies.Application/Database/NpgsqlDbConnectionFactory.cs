using System.Data.Common;
using Npgsql;

namespace Movies.Application.Database;

internal class NpgsqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly DbDataSource _dataSource;

    public NpgsqlDbConnectionFactory(string connectionString)
    {
        _dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
    }

    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }
}