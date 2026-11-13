using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class PeriodRepository : IPeriodRepository
{
    private const string SelectColumns = """
        id as Id,
        period_number as PeriodNumber,
        start_time as StartTime,
        end_time as EndTime,
        period_type as PeriodType,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PeriodRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PeriodDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.periods
            order by period_number;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PeriodDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<PeriodDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.periods
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PeriodDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<PeriodDto> InsertAsync(PeriodUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.periods (period_number, start_time, end_time, period_type, status_id, created_by, modified_by)
            values (@PeriodNumber, @StartTime, @EndTime, @PeriodType, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<PeriodDto>(new CommandDefinition(
            sql,
            new
            {
                request.PeriodNumber,
                request.StartTime,
                request.EndTime,
                request.PeriodType,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<PeriodDto?> UpdateAsync(int id, PeriodUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.periods
            set
                period_number = @PeriodNumber,
                start_time = @StartTime,
                end_time = @EndTime,
                period_type = @PeriodType,
                status_id = @StatusId,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PeriodDto>(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                request.PeriodNumber,
                request.StartTime,
                request.EndTime,
                request.PeriodType,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }
}
