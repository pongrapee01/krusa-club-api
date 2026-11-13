using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface ISubjectRepository
{
    Task<IReadOnlyList<SubjectDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubjectDto> InsertAsync(SubjectUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<SubjectDto?> UpdateAsync(Guid id, SubjectUpsertRequest request, Guid? userId, CancellationToken cancellationToken = default);
}
