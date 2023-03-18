using System.Data.Common;

namespace Movies.Application.Database;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken token = default);
}