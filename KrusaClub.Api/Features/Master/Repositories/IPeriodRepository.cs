using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface IPeriodRepository
{
    Task<IReadOnlyList<PeriodDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PeriodDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PeriodDto> InsertAsync(PeriodUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<PeriodDto?> UpdateAsync(int id, PeriodUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
