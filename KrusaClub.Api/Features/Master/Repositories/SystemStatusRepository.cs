using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class SystemStatusRepository : ISystemStatusRepository
{
    private const string SelectColumns = """
        id as Id,
        name as Name,
        description as Description,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public SystemStatusRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<SystemStatusDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.system_status
            order by id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SystemStatusDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<SystemStatusDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.system_status
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SystemStatusDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<SystemStatusDto> InsertAsync(SystemStatusUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.system_status (name, description, created_by, modified_by)
            values (@Name, @Description, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<SystemStatusDto>(new CommandDefinition(
            sql,
            new { request.Name, request.Description, UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<SystemStatusDto?> UpdateAsync(int id, SystemStatusUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.system_status
            set
                name = @Name,
                description = @Description,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SystemStatusDto>(new CommandDefinition(
            sql,
            new { Id = id, request.Name, request.Description, UserId = userId },
            cancellationToken: cancellationToken));
    }
}
