namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class SystemStatusDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? Modified { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record SystemStatusUpsertRequest(
    string Name,
    string? Description);
