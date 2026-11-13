using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface ISystemStatusRepository
{
    Task<IReadOnlyList<SystemStatusDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SystemStatusDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SystemStatusDto> InsertAsync(SystemStatusUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<SystemStatusDto?> UpdateAsync(int id, SystemStatusUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
