using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class SubjectRepository : ISubjectRepository
{
    private const string SelectColumns = """
        id as Id,
        code as Code,
        name as Name,
        credit as Credit,
        color_hex as ColorHex,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public SubjectRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<SubjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.subjects
            order by code;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SubjectDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<SubjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.subjects
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SubjectDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<SubjectDto> InsertAsync(SubjectUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.subjects (code, name, credit, color_hex, status_id, created_by, modified_by)
            values (@Code, @Name, @Credit, @ColorHex, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<SubjectDto>(new CommandDefinition(
            sql,
            new
            {
                request.Code,
                request.Name,
                request.Credit,
                request.ColorHex,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<SubjectDto?> UpdateAsync(Guid id, SubjectUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.subjects
            set
                code = @Code,
                name = @Name,
                credit = @Credit,
                color_hex = @ColorHex,
                status_id = @StatusId,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SubjectDto>(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                request.Code,
                request.Name,
                request.Credit,
                request.ColorHex,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }
}
