using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface IMasterMenuRepository
{
    Task<IReadOnlyList<MasterMenuDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MasterMenuDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MasterMenuDto> InsertAsync(MasterMenuUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<MasterMenuDto?> UpdateAsync(Guid id, MasterMenuUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
