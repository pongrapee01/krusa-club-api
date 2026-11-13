namespace KrusaClub.Api.Auth;

public interface ICurrentUserAccessor
{
    string GetRequiredUserId();
}
