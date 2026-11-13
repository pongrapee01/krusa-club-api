using KrusaClub.Api.Features.Master.Dtos;

namespace KrusaClub.Api.Features.Master;

public interface IMasterService
{
    Task<IReadOnlyList<TitleDto>> GetTitlesAsync(CancellationToken cancellationToken = default);
    Task<TitleDto?> GetTitleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TitleDto> InsertTitleAsync(TitleUpsertRequest request, CancellationToken cancellationToken = default);
    Task<TitleDto?> UpdateTitleAsync(Guid id, TitleUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MasterRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<MasterRoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MasterRoleDto> InsertRoleAsync(MasterRoleUpsertRequest request, CancellationToken cancellationToken = default);
    Task<MasterRoleDto?> UpdateRoleAsync(Guid id, MasterRoleUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SemesterDto>> GetSemestersAsync(CancellationToken cancellationToken = default);
    Task<SemesterDto?> GetSemesterByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SemesterDto> InsertSemesterAsync(SemesterUpsertRequest request, CancellationToken cancellationToken = default);
    Task<SemesterDto?> UpdateSemesterAsync(Guid id, SemesterUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassDto>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassDto?> GetClassByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassDto> InsertClassAsync(ClassUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ClassDto?> UpdateClassAsync(Guid id, ClassUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default);
    Task<PeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PeriodDto> InsertPeriodAsync(PeriodUpsertRequest request, CancellationToken cancellationToken = default);
    Task<PeriodDto?> UpdatePeriodAsync(int id, PeriodUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubjectDto>> GetSubjectsAsync(CancellationToken cancellationToken = default);
    Task<SubjectDto?> GetSubjectByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubjectDto> InsertSubjectAsync(SubjectUpsertRequest request, CancellationToken cancellationToken = default);
    Task<SubjectDto?> UpdateSubjectAsync(Guid id, SubjectUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceStatusDto>> GetAttendanceStatusesAsync(CancellationToken cancellationToken = default);
    Task<AttendanceStatusDto?> GetAttendanceStatusByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AttendanceStatusDto> InsertAttendanceStatusAsync(AttendanceStatusInsertRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceStatusDto?> UpdateAttendanceStatusAsync(int id, AttendanceStatusUpdateRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemStatusDto>> GetSystemStatusesAsync(CancellationToken cancellationToken = default);
    Task<SystemStatusDto?> GetSystemStatusByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SystemStatusDto> InsertSystemStatusAsync(SystemStatusUpsertRequest request, CancellationToken cancellationToken = default);
    Task<SystemStatusDto?> UpdateSystemStatusAsync(int id, SystemStatusUpsertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MasterMenuDto>> GetMenusAsync(CancellationToken cancellationToken = default);
    Task<MasterMenuDto?> GetMenuByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MasterMenuDto> InsertMenuAsync(MasterMenuUpsertRequest request, CancellationToken cancellationToken = default);
    Task<MasterMenuDto?> UpdateMenuAsync(Guid id, MasterMenuUpsertRequest request, CancellationToken cancellationToken = default);
}
