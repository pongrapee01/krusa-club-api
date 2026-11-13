using KrusaClub.Api.Auth;
using KrusaClub.Api.Features.Master.Caching;
using KrusaClub.Api.Features.Master.Dtos;
using KrusaClub.Api.Features.Master.Repositories;

namespace KrusaClub.Api.Features.Master;

public sealed class MasterService : IMasterService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly MasterDataCache _cache;
    private readonly ITitleRepository _titleRepository;
    private readonly IMasterRoleRepository _roleRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IClassRepository _classRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IAttendanceStatusRepository _attendanceStatusRepository;
    private readonly ISystemStatusRepository _systemStatusRepository;
    private readonly IMasterMenuRepository _menuRepository;

    public MasterService(
        ICurrentUserAccessor currentUserAccessor,
        MasterDataCache cache,
        ITitleRepository titleRepository,
        IMasterRoleRepository roleRepository,
        ISemesterRepository semesterRepository,
        IClassRepository classRepository,
        IPeriodRepository periodRepository,
        ISubjectRepository subjectRepository,
        IAttendanceStatusRepository attendanceStatusRepository,
        ISystemStatusRepository systemStatusRepository,
        IMasterMenuRepository menuRepository)
    {
        _currentUserAccessor = currentUserAccessor;
        _cache = cache;
        _titleRepository = titleRepository;
        _roleRepository = roleRepository;
        _semesterRepository = semesterRepository;
        _classRepository = classRepository;
        _periodRepository = periodRepository;
        _subjectRepository = subjectRepository;
        _attendanceStatusRepository = attendanceStatusRepository;
        _systemStatusRepository = systemStatusRepository;
        _menuRepository = menuRepository;
    }

    public Task<IReadOnlyList<TitleDto>> GetTitlesAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Titles, _titleRepository.GetAllAsync, cancellationToken);

    public async Task<TitleDto?> GetTitleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await GetTitlesAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _titleRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<TitleDto> InsertTitleAsync(TitleUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _titleRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Titles);
        return result;
    }

    public async Task<TitleDto?> UpdateTitleAsync(Guid id, TitleUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _titleRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Titles);
        }

        return result;
    }

    public Task<IReadOnlyList<MasterRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Roles, _roleRepository.GetAllAsync, cancellationToken);

    public async Task<MasterRoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await GetRolesAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _roleRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<MasterRoleDto> InsertRoleAsync(MasterRoleUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _roleRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Roles);
        return result;
    }

    public async Task<MasterRoleDto?> UpdateRoleAsync(Guid id, MasterRoleUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _roleRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Roles);
        }

        return result;
    }

    public Task<IReadOnlyList<SemesterDto>> GetSemestersAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Semesters, _semesterRepository.GetAllAsync, cancellationToken);

    public async Task<SemesterDto?> GetSemesterByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await GetSemestersAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _semesterRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<SemesterDto> InsertSemesterAsync(SemesterUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _semesterRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Semesters);
        return result;
    }

    public async Task<SemesterDto?> UpdateSemesterAsync(Guid id, SemesterUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _semesterRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Semesters);
        }

        return result;
    }

    public Task<IReadOnlyList<ClassDto>> GetClassesAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Classes, _classRepository.GetAllAsync, cancellationToken);

    public async Task<ClassDto?> GetClassByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await GetClassesAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _classRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ClassDto> InsertClassAsync(ClassUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _classRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Classes);
        return result;
    }

    public async Task<ClassDto?> UpdateClassAsync(Guid id, ClassUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _classRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Classes);
        }

        return result;
    }

    public Task<IReadOnlyList<PeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Periods, _periodRepository.GetAllAsync, cancellationToken);

    public async Task<PeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cached = await GetPeriodsAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _periodRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<PeriodDto> InsertPeriodAsync(PeriodUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _periodRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Periods);
        return result;
    }

    public async Task<PeriodDto?> UpdatePeriodAsync(int id, PeriodUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _periodRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Periods);
        }

        return result;
    }

    public Task<IReadOnlyList<SubjectDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Subjects, _subjectRepository.GetAllAsync, cancellationToken);

    public async Task<SubjectDto?> GetSubjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await GetSubjectsAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _subjectRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<SubjectDto> InsertSubjectAsync(SubjectUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _subjectRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Subjects);
        return result;
    }

    public async Task<SubjectDto?> UpdateSubjectAsync(Guid id, SubjectUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _subjectRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Subjects);
        }

        return result;
    }

    public Task<IReadOnlyList<AttendanceStatusDto>> GetAttendanceStatusesAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.AttendanceStatuses, _attendanceStatusRepository.GetAllAsync, cancellationToken);

    public async Task<AttendanceStatusDto?> GetAttendanceStatusByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cached = await GetAttendanceStatusesAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _attendanceStatusRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<AttendanceStatusDto> InsertAttendanceStatusAsync(
        AttendanceStatusInsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _attendanceStatusRepository.InsertAsync(request, cancellationToken);
        _cache.Invalidate(MasterCacheKeys.AttendanceStatuses);
        return result;
    }

    public async Task<AttendanceStatusDto?> UpdateAttendanceStatusAsync(
        int id,
        AttendanceStatusUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _attendanceStatusRepository.UpdateAsync(id, request, cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.AttendanceStatuses);
        }

        return result;
    }

    public Task<IReadOnlyList<SystemStatusDto>> GetSystemStatusesAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.SystemStatuses, _systemStatusRepository.GetAllAsync, cancellationToken);

    public async Task<SystemStatusDto?> GetSystemStatusByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cached = await GetSystemStatusesAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _systemStatusRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<SystemStatusDto> InsertSystemStatusAsync(SystemStatusUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _systemStatusRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.SystemStatuses);
        return result;
    }

    public async Task<SystemStatusDto?> UpdateSystemStatusAsync(int id, SystemStatusUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _systemStatusRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.SystemStatuses);
        }

        return result;
    }

    public Task<IReadOnlyList<MasterMenuDto>> GetMenusAsync(CancellationToken cancellationToken = default)
        => _cache.GetListAsync(MasterCacheKeys.Menus, _menuRepository.GetAllAsync, cancellationToken);

    public async Task<MasterMenuDto?> GetMenuByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await GetMenusAsync(cancellationToken);
        var hit = cached.FirstOrDefault(x => x.Id == id);
        if (hit is not null)
        {
            return hit;
        }

        return await _menuRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<MasterMenuDto> InsertMenuAsync(MasterMenuUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _menuRepository.InsertAsync(
            request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        _cache.Invalidate(MasterCacheKeys.Menus);
        return result;
    }

    public async Task<MasterMenuDto?> UpdateMenuAsync(Guid id, MasterMenuUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _menuRepository.UpdateAsync(
            id, request, MasterUserContext.TryGetUserId(_currentUserAccessor), cancellationToken);
        if (result is not null)
        {
            _cache.Invalidate(MasterCacheKeys.Menus);
        }

        return result;
    }
}
