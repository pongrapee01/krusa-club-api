using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface IMasterRoleRepository
{
    Task<IReadOnlyList<MasterRoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MasterRoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MasterRoleDto> InsertAsync(MasterRoleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<MasterRoleDto?> UpdateAsync(Guid id, MasterRoleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
