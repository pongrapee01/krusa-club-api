using KrusaClub.Api.Auth;

namespace KrusaClub.Api.Features.Master;

internal static class MasterUserContext
{
    public static Guid? TryGetUserId(ICurrentUserAccessor currentUserAccessor)
    {
        var raw = currentUserAccessor.GetRequiredUserId();
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }
}
