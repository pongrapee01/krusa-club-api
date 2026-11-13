namespace KrusaClub.Api.Features.Users;

public sealed record UserProfileDto(
    string UserId,
    string? DisplayName,
    string? Email,
    DateTimeOffset? UpdatedAt
);
