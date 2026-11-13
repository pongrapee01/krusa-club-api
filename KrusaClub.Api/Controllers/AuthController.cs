using System.Security.Claims;
using KrusaClub.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KrusaClub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AuthController(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = _currentUserAccessor.GetRequiredUserId();

        var response = new
        {
            userId,
            email = User.FindFirstValue("email"),
            role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role"),
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        };

        return Ok(response);
    }
}
