using Dapper;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Users;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserProfileRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserProfileDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           select
                               id as UserId,
                               display_name as DisplayName,
                               email as Email,
                               updated_at as UpdatedAt
                           from public.profiles
                           where id = @UserId
                           limit 1;
                           """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserProfileDto>(new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken));
    }
}
