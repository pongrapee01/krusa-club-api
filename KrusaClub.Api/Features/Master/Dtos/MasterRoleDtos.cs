namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class MasterRoleDto
{
    public Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int StatusId { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? ModifiedAt { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record MasterRoleUpsertRequest(
    string Code,
    string Name,
    string? Description,
    int StatusId = 1);
