using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class MasterMenuRepository : IMasterMenuRepository
{
    private const string SelectColumns = """
        id as Id,
        code as Code,
        label as Label,
        parent_code as ParentCode,
        path as Path,
        type as Type,
        description as Description,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public MasterMenuRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MasterMenuDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.menus
            order by type nulls last, code;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<MasterMenuDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<MasterMenuDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.menus
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<MasterMenuDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<MasterMenuDto> InsertAsync(MasterMenuUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.menus (code, label, parent_code, path, type, description, status_id, created_by, modified_by)
            values (@Code, @Label, @ParentCode, @Path, @Type, @Description, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<MasterMenuDto>(new CommandDefinition(
            sql,
            new
            {
                request.Code,
                request.Label,
                request.ParentCode,
                request.Path,
                request.Type,
                request.Description,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<MasterMenuDto?> UpdateAsync(Guid id, MasterMenuUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.menus
            set
                code = @Code,
                label = @Label,
                parent_code = @ParentCode,
                path = @Path,
                type = @Type,
                description = @Description,
                status_id = @StatusId,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<MasterMenuDto>(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                request.Code,
                request.Label,
                request.ParentCode,
                request.Path,
                request.Type,
                request.Description,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }
}
