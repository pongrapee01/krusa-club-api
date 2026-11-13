namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class SubjectDto
{
    public Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public decimal Credit { get; init; }
    public required string ColorHex { get; init; }
    public int StatusId { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? Modified { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record SubjectUpsertRequest(
    string Code,
    string Name,
    decimal Credit = 1.0m,
    string ColorHex = "#E2E8F0",
    int StatusId = 1);
