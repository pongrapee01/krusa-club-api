using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class MasterRoleRepository : IMasterRoleRepository
{
    private const string SelectColumns = """
        id as Id,
        code as Code,
        name as Name,
        description as Description,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified_at as ModifiedAt,
        modified as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public MasterRoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MasterRoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = $"""
            select {SelectColumns}
            from public.roles
            order by code;
            """;

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
            var rows = await connection.QueryAsync<MasterRoleDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return rows.AsList();
        }
        catch (Exception e)
        { 
            // Log the exception (you can use your preferred logging framework)
            Console.Error.WriteLine($"Error fetching roles: {e.Message}");
            // Optionally, rethrow the exception or return an empty list
            throw;
        }
    }

    public async Task<MasterRoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.roles
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<MasterRoleDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<MasterRoleDto> InsertAsync(MasterRoleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.roles (code, name, description, status_id, created_by, modified)
            values (@Code, @Name, @Description, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<MasterRoleDto>(new CommandDefinition(
            sql,
            new { request.Code, request.Name, request.Description, request.StatusId, UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<MasterRoleDto?> UpdateAsync(Guid id, MasterRoleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.roles
            set
                code = @Code,
                name = @Name,
                description = @Description,
                status_id = @StatusId,
                modified_at = now(),
                modified = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<MasterRoleDto>(new CommandDefinition(
            sql,
            new { Id = id, request.Code, request.Name, request.Description, request.StatusId, UserId = userId },
            cancellationToken: cancellationToken));
    }
}
