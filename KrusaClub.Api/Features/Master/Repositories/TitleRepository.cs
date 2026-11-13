using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class TitleRepository : ITitleRepository
{
    private const string SelectColumns = """
        id as Id,
        name as Name,
        description as Description,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public TitleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TitleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = $"""
            select {SelectColumns}
            from public.titles
            order by name;
            """;

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
            var rows = await connection.QueryAsync<TitleDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return rows.AsList();
        }
        catch (Exception e)
        {
            // Log the exception (you can use your preferred logging framework)
            Console.Error.WriteLine($"Error fetching titles: {e.Message}");
            // Optionally, rethrow the exception or return an empty list
            throw;
        }
    }

    public async Task<TitleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.titles
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TitleDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<TitleDto> InsertAsync(TitleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.titles (name, description, status_id, created_by, modified_by)
            values (@Name, @Description, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<TitleDto>(new CommandDefinition(
            sql,
            new { request.Name, request.Description, request.StatusId, UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<TitleDto?> UpdateAsync(Guid id, TitleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.titles
            set
                name = @Name,
                description = @Description,
                status_id = @StatusId,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TitleDto>(new CommandDefinition(
            sql,
            new { Id = id, request.Name, request.Description, request.StatusId, UserId = userId },
            cancellationToken: cancellationToken));
    }
}
