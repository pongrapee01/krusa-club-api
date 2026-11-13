using KrusaClub.Api.Auth;
using KrusaClub.Api.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KrusaClub.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUserProfileRepository _userProfileRepository;

    public MeController(ICurrentUserAccessor currentUserAccessor, IUserProfileRepository userProfileRepository)
    {
        _currentUserAccessor = currentUserAccessor;
        _userProfileRepository = userProfileRepository;
    }

    [HttpGet("id")]
    public IActionResult GetMyId()
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        return Ok(new { userId });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile is null)
        {
            return NotFound(new { message = $"No profile found for user {userId}." });
        }

        return Ok(profile);
    }
}
