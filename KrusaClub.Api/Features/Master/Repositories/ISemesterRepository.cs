using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface ISemesterRepository
{
    Task<IReadOnlyList<SemesterDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SemesterDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SemesterDto> InsertAsync(SemesterUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<SemesterDto?> UpdateAsync(Guid id, SemesterUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
