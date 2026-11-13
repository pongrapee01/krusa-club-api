using Npgsql;

namespace KrusaClub.Api.Infrastructure.Data;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dataSource.OpenConnectionAsync(cancellationToken);
        }
        catch (Exception ex) when (IsTransientConnectionReadFailure(ex))
        {
            // A broken pooled connector can produce "Exception while reading from stream".
            // Clear pool and retry once with a fresh physical connection.
            _dataSource.Clear();
            return await _dataSource.OpenConnectionAsync(cancellationToken);
        }
    }

    private static bool IsTransientConnectionReadFailure(Exception ex)
    {
        if (ex is TimeoutException)
        {
            return true;
        }

        if (ex is NpgsqlException npgsqlEx &&
            npgsqlEx.Message.Contains("reading from stream", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ex.Message.Contains("reading from stream", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ex.InnerException is not null && IsTransientConnectionReadFailure(ex.InnerException);
    }
}
