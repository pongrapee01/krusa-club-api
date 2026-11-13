namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class PeriodDto
{
    public int Id { get; init; }
    public int PeriodNumber { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public required string PeriodType { get; init; }
    public int StatusId { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? Modified { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record PeriodUpsertRequest(
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string PeriodType = "CLASS",
    int StatusId = 1);
