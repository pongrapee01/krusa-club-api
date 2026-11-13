namespace KrusaClub.Api.Features.Users;

public interface IUserProfileRepository
{
    Task<UserProfileDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
