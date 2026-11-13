using Dapper;
using KrusaClub.Api.Infrastructure.Data;

namespace KrusaClub.Api.Features.Menus;

public sealed class MenuRepository : IMenuRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MenuRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetAuthorizedMenusAsync(string userId,CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = """
                           select 
                               m.id as Id,
                               m.code as Code,
                               m.label as Label,
                               coalesce(m.path, '') as Path,
                               m.icon as Icon,
                               m.type as Type,
                               m.parent_code as ParentCode,
                               m.sort_order as SortOrder
                           from public.user_roles ur
                           inner join public.role_menus rm on rm.role_id = ur.role_id
                           inner join public.menus m on m.id = rm.menu_id
                           where ur.user_profile_id = @UserId::uuid;                          
                           """;

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
            var rows = (await connection.QueryAsync<MenuRow>(new CommandDefinition(
                sql,
                new { UserId = userId },
                cancellationToken: cancellationToken))).ToList();

            var topbars = rows
                .Where(r => string.Equals(r.Type, "TOPBAR", StringComparison.OrdinalIgnoreCase))
                .Select(r => new MenuItemDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    Label = r.Label,
                    Path = r.Path,
                    Icon = r.Icon,
                    SortOrder = r.SortOrder,
                    SubMenus = []
                })
                .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

            var orphanBucket = new List<MenuItemDto>();

            foreach (var row in rows.Where(r => string.Equals(r.Type, "SIDEBAR", StringComparison.OrdinalIgnoreCase)))
            {
                var sidebar = new MenuItemDto
                {
                    Id = row.Id,
                    Code = row.Code,
                    Label = row.Label,
                    Path = row.Path,
                    Icon = row.Icon,
                    SortOrder = row.SortOrder,
                    SubMenus = []
                };

                if (!string.IsNullOrWhiteSpace(row.ParentCode) && topbars.TryGetValue(row.ParentCode, out var parent))
                {
                    parent.SubMenus.Add(sidebar);
                    continue;
                }

                orphanBucket.Add(sidebar);
            }

            if (orphanBucket.Count > 0)
            {
                var defaultTopbar = new MenuItemDto
                {
                    Id = Guid.Empty,
                    Code = "__ORPHAN__",
                    Label = "General",
                    Path = string.Empty,
                    Icon = null,
                    SubMenus = orphanBucket
                };
                topbars[defaultTopbar.Code] = defaultTopbar;
            }

            return topbars.Values.ToList();
        }
        catch
        {
            return new List<MenuItemDto> { };
        }
    }

    private sealed class MenuRow
    {
        public required Guid Id { get; init; }
        public required string Code { get; init; }
        public required string Label { get; init; }
        public required string Path { get; init; }
        public string? Icon { get; init; }
        public required string Type { get; init; }
        public int? SortOrder { get; init; }
        public string? ParentCode { get; init; }
    }
}
