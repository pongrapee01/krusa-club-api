using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface ITitleRepository
{
    Task<IReadOnlyList<TitleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TitleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TitleDto> InsertAsync(TitleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<TitleDto?> UpdateAsync(Guid id, TitleUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
