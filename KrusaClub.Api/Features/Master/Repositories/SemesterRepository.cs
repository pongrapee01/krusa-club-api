using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class SemesterRepository : ISemesterRepository
{
    private const string SelectColumns = """
        id as Id,
        academic_year as AcademicYear,
        term as Term,
        start_date as StartDate,
        end_date as EndDate,
        is_current as IsCurrent,
        status_id as StatusId,
        created as Created,
        created_by as CreatedBy,
        modified as Modified,
        modified_by as ModifiedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public SemesterRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<SemesterDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.semesters
            order by academic_year desc, term;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SemesterDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<SemesterDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.semesters
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SemesterDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<SemesterDto> InsertAsync(SemesterUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.semesters (academic_year, term, start_date, end_date, is_current, status_id, created_by, modified_by)
            values (@AcademicYear, @Term, @StartDate, @EndDate, @IsCurrent, @StatusId, @UserId, @UserId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<SemesterDto>(new CommandDefinition(
            sql,
            new
            {
                request.AcademicYear,
                request.Term,
                request.StartDate,
                request.EndDate,
                request.IsCurrent,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<SemesterDto?> UpdateAsync(Guid id, SemesterUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.semesters
            set
                academic_year = @AcademicYear,
                term = @Term,
                start_date = @StartDate,
                end_date = @EndDate,
                is_current = @IsCurrent,
                status_id = @StatusId,
                modified = now(),
                modified_by = @UserId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SemesterDto>(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                request.AcademicYear,
                request.Term,
                request.StartDate,
                request.EndDate,
                request.IsCurrent,
                request.StatusId,
                UserId = userId
            },
            cancellationToken: cancellationToken));
    }
}
