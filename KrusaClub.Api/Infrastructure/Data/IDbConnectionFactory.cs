namespace KrusaClub.Api.Infrastructure.Data;

public interface IDbConnectionFactory
{
    Task<Npgsql.NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
