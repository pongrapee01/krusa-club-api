namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class ClassDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Level { get; init; }
    public int StatusId { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? Modified { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record ClassUpsertRequest(
    string Name,
    string? Level,
    int StatusId = 1);
