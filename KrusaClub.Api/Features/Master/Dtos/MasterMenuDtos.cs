namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class MasterMenuDto
{
    public Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Label { get; init; }
    public string? ParentCode { get; init; }
    public string? Path { get; init; }
    public string? Type { get; init; }
    public string? Description { get; init; }
    public int StatusId { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? Modified { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record MasterMenuUpsertRequest(
    string Code,
    string Label,
    string? ParentCode,
    string? Path,
    string? Type,
    string? Description,
    int StatusId = 1);
