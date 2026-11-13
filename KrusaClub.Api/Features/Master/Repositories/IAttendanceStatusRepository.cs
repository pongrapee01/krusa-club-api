using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master.Repositories;

public interface IAttendanceStatusRepository
{
    Task<IReadOnlyList<AttendanceStatusDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AttendanceStatusDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AttendanceStatusDto> InsertAsync(AttendanceStatusInsertRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceStatusDto?> UpdateAsync(int id, AttendanceStatusUpdateRequest request, CancellationToken cancellationToken = default);
}
