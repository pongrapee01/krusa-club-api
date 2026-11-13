namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class AttendanceStatusDto
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string NameTh { get; init; }
    public required string ColorHex { get; init; }
    public string? IconName { get; init; }
    public int StatusId { get; init; }
}

public sealed record AttendanceStatusInsertRequest(
    int Id,
    string Code,
    string NameTh,
    string ColorHex,
    string? IconName,
    int StatusId = 1);

public sealed record AttendanceStatusUpdateRequest(
    string Code,
    string NameTh,
    string ColorHex,
    string? IconName,
    int StatusId = 1);
