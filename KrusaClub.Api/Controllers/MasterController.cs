using KrusaClub.Api.Features.Master;
using KrusaClub.Api.Features.Master.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KrusaClub.Api.Controllers;

[ApiController]
[Route("api/master")]
[Authorize]
public sealed class MasterController : ControllerBase
{
    private readonly IMasterService _masterService;

    public MasterController(IMasterService masterService)
    {
        _masterService = masterService;
    }

    [HttpGet("titles")]
    public async Task<IActionResult> GetTitles(CancellationToken cancellationToken)
        => Ok(await _masterService.GetTitlesAsync(cancellationToken));

    [HttpGet("titles/{id:guid}")]
    public async Task<IActionResult> GetTitle(Guid id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetTitleByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("titles")]
    public async Task<IActionResult> InsertTitle([FromBody] TitleUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertTitleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTitle), new { id = result.Id }, result);
    }

    [HttpPut("titles/{id:guid}")]
    public async Task<IActionResult> UpdateTitle(Guid id, [FromBody] TitleUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateTitleAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        => Ok(await _masterService.GetRolesAsync(cancellationToken));

    [HttpGet("roles/{id:guid}")]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetRoleByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("roles")]
    public async Task<IActionResult> InsertRole([FromBody] MasterRoleUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRole), new { id = result.Id }, result);
    }

    [HttpPut("roles/{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] MasterRoleUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateRoleAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("semesters")]
    public async Task<IActionResult> GetSemesters(CancellationToken cancellationToken)
        => Ok(await _masterService.GetSemestersAsync(cancellationToken));

    [HttpGet("semesters/{id:guid}")]
    public async Task<IActionResult> GetSemester(Guid id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetSemesterByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("semesters")]
    public async Task<IActionResult> InsertSemester([FromBody] SemesterUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertSemesterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSemester), new { id = result.Id }, result);
    }

    [HttpPut("semesters/{id:guid}")]
    public async Task<IActionResult> UpdateSemester(Guid id, [FromBody] SemesterUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateSemesterAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
        => Ok(await _masterService.GetClassesAsync(cancellationToken));

    [HttpGet("classes/{id:guid}")]
    public async Task<IActionResult> GetClass(Guid id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetClassByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("classes")]
    public async Task<IActionResult> InsertClass([FromBody] ClassUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertClassAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetClass), new { id = result.Id }, result);
    }

    [HttpPut("classes/{id:guid}")]
    public async Task<IActionResult> UpdateClass(Guid id, [FromBody] ClassUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateClassAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("periods")]
    public async Task<IActionResult> GetPeriods(CancellationToken cancellationToken)
        => Ok(await _masterService.GetPeriodsAsync(cancellationToken));

    [HttpGet("periods/{id:int}")]
    public async Task<IActionResult> GetPeriod(int id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetPeriodByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("periods")]
    public async Task<IActionResult> InsertPeriod([FromBody] PeriodUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertPeriodAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPeriod), new { id = result.Id }, result);
    }

    [HttpPut("periods/{id:int}")]
    public async Task<IActionResult> UpdatePeriod(int id, [FromBody] PeriodUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdatePeriodAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects(CancellationToken cancellationToken)
        => Ok(await _masterService.GetSubjectsAsync(cancellationToken));

    [HttpGet("subjects/{id:guid}")]
    public async Task<IActionResult> GetSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetSubjectByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("subjects")]
    public async Task<IActionResult> InsertSubject([FromBody] SubjectUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertSubjectAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSubject), new { id = result.Id }, result);
    }

    [HttpPut("subjects/{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] SubjectUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateSubjectAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("attendance-statuses")]
    public async Task<IActionResult> GetAttendanceStatuses(CancellationToken cancellationToken)
        => Ok(await _masterService.GetAttendanceStatusesAsync(cancellationToken));

    [HttpGet("attendance-statuses/{id:int}")]
    public async Task<IActionResult> GetAttendanceStatus(int id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetAttendanceStatusByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("attendance-statuses")]
    public async Task<IActionResult> InsertAttendanceStatus([FromBody] AttendanceStatusInsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertAttendanceStatusAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAttendanceStatus), new { id = result.Id }, result);
    }

    [HttpPut("attendance-statuses/{id:int}")]
    public async Task<IActionResult> UpdateAttendanceStatus(int id, [FromBody] AttendanceStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateAttendanceStatusAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("system-statuses")]
    public async Task<IActionResult> GetSystemStatuses(CancellationToken cancellationToken)
        => Ok(await _masterService.GetSystemStatusesAsync(cancellationToken));

    [HttpGet("system-statuses/{id:int}")]
    public async Task<IActionResult> GetSystemStatus(int id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetSystemStatusByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("system-statuses")]
    public async Task<IActionResult> InsertSystemStatus([FromBody] SystemStatusUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertSystemStatusAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSystemStatus), new { id = result.Id }, result);
    }

    [HttpPut("system-statuses/{id:int}")]
    public async Task<IActionResult> UpdateSystemStatus(int id, [FromBody] SystemStatusUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateSystemStatusAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("menus")]
    public async Task<IActionResult> GetMenus(CancellationToken cancellationToken)
        => Ok(await _masterService.GetMenusAsync(cancellationToken));

    [HttpGet("menus/{id:guid}")]
    public async Task<IActionResult> GetMenu(Guid id, CancellationToken cancellationToken)
    {
        var result = await _masterService.GetMenuByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("menus")]
    public async Task<IActionResult> InsertMenu([FromBody] MasterMenuUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.InsertMenuAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMenu), new { id = result.Id }, result);
    }

    [HttpPut("menus/{id:guid}")]
    public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] MasterMenuUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await _masterService.UpdateMenuAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
