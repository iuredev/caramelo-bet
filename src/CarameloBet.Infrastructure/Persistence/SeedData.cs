using CarameloBet.Domain.Entities.Auth;
using CarameloBet.Infrastructure.Persistence.Auth;
using CarameloBet.Infrastructure.Persistence.Game;
using Microsoft.EntityFrameworkCore;
using GameEntity = CarameloBet.Domain.Entities.Game.Game;
using TableEntity = CarameloBet.Domain.Entities.Game.Table;

namespace CarameloBet.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task SeedAsync(AuthDbContext authContext, GameDbContext gameContext)
        {
            await SeedRolesAndPermissionsAsync(authContext);
            await SeedGamesAndTablesAsync(gameContext);
        }

        private static async Task SeedRolesAndPermissionsAsync(AuthDbContext authContext)
        {
            if (await authContext.Roles.AnyAsync()) return;

            var playerRole = Role.Create("player", "Regular player");
            var adminRole = Role.Create("admin", "Platform administrator");
            var moderatorRole = Role.Create("moderator", "Game moderator");
            var supportRole = Role.Create("support", "Customer support");

            await authContext.Roles.AddRangeAsync(playerRole, adminRole, moderatorRole, supportRole);

            var permissions = new[]
            {
                        Permission.Create("can_play_games", "Can play games"),
                        Permission.Create("can_view_own_history", "Can view own history"),
                        Permission.Create("can_view_player_history", "Can view any player history"),
                        Permission.Create("can_ban_player", "Can ban a player"),
                        Permission.Create("can_manage_tables", "Can manage game tables"),
                        Permission.Create("can_manage_users", "Can manage users"),
                        Permission.Create("can_view_all_reports", "Can view all reports"),
                        Permission.Create("can_manage_roles", "Can manage roles and permissions"),
                    };

            await authContext.Permissions.AddRangeAsync(permissions);
            await authContext.SaveChangesAsync();

            var p = permissions.ToDictionary(x => x.Name);

            await authContext.RolePermissions.AddRangeAsync(
                RolePermission.Create(playerRole.Id, p["can_play_games"].Id),
                RolePermission.Create(playerRole.Id, p["can_view_own_history"].Id)
            );

            await authContext.RolePermissions.AddRangeAsync(
                RolePermission.Create(supportRole.Id, p["can_view_player_history"].Id),
                RolePermission.Create(supportRole.Id, p["can_view_all_reports"].Id)
            );

            await authContext.RolePermissions.AddRangeAsync(
                RolePermission.Create(moderatorRole.Id, p["can_view_player_history"].Id),
                RolePermission.Create(moderatorRole.Id, p["can_ban_player"].Id),
                RolePermission.Create(moderatorRole.Id, p["can_view_all_reports"].Id)
            );



            foreach (var permission in permissions)
                await authContext.RolePermissions.AddAsync(
                    RolePermission.Create(adminRole.Id, permission.Id));

            await authContext.SaveChangesAsync();
        }

        private static async Task SeedGamesAndTablesAsync(GameDbContext gameContext)
        {
            if (await gameContext.Games.AnyAsync()) return;

            var roulette = GameEntity.Create("European Roulette", "roulette", 10m, 1000m);
            var aviator = GameEntity.Create("Caramelinho", "aviator", 1m, 10000m);

            await gameContext.Games.AddRangeAsync(roulette, aviator);
            await gameContext.SaveChangesAsync();

            await gameContext.Tables.AddRangeAsync(
                        TableEntity.Create(roulette.Id, "Roulette Table 1"),
                        TableEntity.Create(roulette.Id, "Roulette Table 2"),
                        TableEntity.Create(aviator.Id, "Aviator Table 1")
                    );

            await gameContext.SaveChangesAsync();

        }
    }
}
