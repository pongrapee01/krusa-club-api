namespace KrusaClub.Api.Features.Master.Dtos;

public sealed class SemesterDto
{
    public Guid Id { get; init; }
    public required string AcademicYear { get; init; }
    public required string Term { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool IsCurrent { get; init; }
    public int StatusId { get; init; }
    public DateTimeOffset Created { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset? Modified { get; init; }
    public Guid? ModifiedBy { get; init; }
}

public sealed record SemesterUpsertRequest(
    string AcademicYear,
    string Term,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent = false,
    int StatusId = 1);
