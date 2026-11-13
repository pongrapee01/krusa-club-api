namespace KrusaClub.Api.Features.Menus;

public interface IMenuRepository
{
    Task<IReadOnlyList<MenuItemDto>> GetAuthorizedMenusAsync(string userId, CancellationToken cancellationToken = default);
}
