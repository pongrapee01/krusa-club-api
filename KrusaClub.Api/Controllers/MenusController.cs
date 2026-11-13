using KrusaClub.Api.Auth;
using KrusaClub.Api.Features.Menus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KrusaClub.Api.Controllers;

[ApiController]
[Route("api/menus")]
[Authorize]
public sealed class MenusController : ControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IMenuRepository _menuRepository;

    public MenusController(ICurrentUserAccessor currentUserAccessor, IMenuRepository menuRepository)
    {
        _currentUserAccessor = currentUserAccessor;
        _menuRepository = menuRepository;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyMenus(CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        var menus = await _menuRepository.GetAuthorizedMenusAsync(userId, cancellationToken);
        return Ok(menus);
    }
}
