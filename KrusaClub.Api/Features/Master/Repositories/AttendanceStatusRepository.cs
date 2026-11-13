using Dapper;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Master.Repositories;

public sealed class AttendanceStatusRepository : IAttendanceStatusRepository
{
    private const string SelectColumns = """
        id as Id,
        code as Code,
        name_th as NameTh,
        color_hex as ColorHex,
        icon_name as IconName,
        status_id as StatusId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public AttendanceStatusRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AttendanceStatusDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.attendance_status
            order by id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<AttendanceStatusDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<AttendanceStatusDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            select {SelectColumns}
            from public.attendance_status
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AttendanceStatusDto>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<AttendanceStatusDto> InsertAsync(AttendanceStatusInsertRequest request, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            insert into public.attendance_status (id, code, name_th, color_hex, icon_name, status_id)
            values (@Id, @Code, @NameTh, @ColorHex, @IconName, @StatusId)
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<AttendanceStatusDto>(new CommandDefinition(
            sql,
            new
            {
                request.Id,
                request.Code,
                request.NameTh,
                request.ColorHex,
                request.IconName,
                request.StatusId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<AttendanceStatusDto?> UpdateAsync(int id, AttendanceStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            update public.attendance_status
            set
                code = @Code,
                name_th = @NameTh,
                color_hex = @ColorHex,
                icon_name = @IconName,
                status_id = @StatusId
            where id = @Id
            returning {SelectColumns};
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AttendanceStatusDto>(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                request.Code,
                request.NameTh,
                request.ColorHex,
                request.IconName,
                request.StatusId
            },
            cancellationToken: cancellationToken));
    }
}
