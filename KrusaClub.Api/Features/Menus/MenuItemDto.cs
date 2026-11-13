namespace KrusaClub.Api.Features.Menus;

public sealed class MenuItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Label { get; init; }
    public required string Path { get; init; }
    public string? Icon { get; init; }
    public int? SortOrder { get; init; }
    public required List<MenuItemDto> SubMenus { get; init; }
}
