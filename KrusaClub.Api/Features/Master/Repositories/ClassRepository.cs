using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class ClassRepository : IClassRepository
{
    private const string SelectColumns = """
        id as Id,
        name as Name,
        level as Level,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public ClassRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ClassDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.classes
            order by name;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ClassDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<ClassDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.classes
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ClassDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<ClassDto> InsertAsync(ClassUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.classes (name, level, status_id, created_by, modified_by)
            values (@Name, @Level, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<ClassDto>(new CommandDefinition(
            sql,
            new { request.Name, request.Level, request.StatusId, UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<ClassDto?> UpdateAsync(Guid id, ClassUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.classes
            set
                name = @Name,
                level = @Level,
                status_id = @StatusId,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ClassDto>(new CommandDefinition(
            sql,
            new { Id = id, request.Name, request.Level, request.StatusId, UserId = userId },
            cancellationToken: cancellationToken));
    }
}
