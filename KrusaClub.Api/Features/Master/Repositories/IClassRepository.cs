using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface IClassRepository
{
    Task<IReadOnlyList<ClassDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassDto> InsertAsync(ClassUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<ClassDto?> UpdateAsync(Guid id, ClassUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
