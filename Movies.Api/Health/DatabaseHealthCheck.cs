using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    public const string Name = "Database";
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
    {
        try
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var result = await connection.QueryAsync(new CommandDefinition("""
            SELECT 1;
            """, cancellationToken: token));

            return result.Count() == 1 ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded();
        }
        catch (Exception e)
        {
            const string message = "Database connection failed";
            _logger.LogError(e, message);
            return HealthCheckResult.Unhealthy(message, e);
        }
    }
}