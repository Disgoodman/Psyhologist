using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Psychologist.Server.Configuration;

namespace Psychologist.Server.Database;

public class DatabaseInitializer(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<AuthConfig> authConfiguration,
    ILogger<DatabaseInitializer> logger)
{
    public async Task Initialize()
    {
        await EnsureRolesCreated();
        await EnsureAdminUserCreated();
    }

    private async Task EnsureRolesCreated()
    {
        foreach (var roleName in Roles.List)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (roleExist) continue;

            var result = await roleManager.CreateAsync(new(roleName));
            logger.LogIdentityResultError(result, $"Unsuccessful role '{roleName}' creation");
        }
    }

    private async Task EnsureAdminUserCreated()
    {
        var user = new ApplicationUser
        {
            Email = authConfiguration.Value.FirstUser.Email,
            UserName = authConfiguration.Value.FirstUser.Email
        };

        var userPassword = authConfiguration.Value.FirstUser.Password;
        var existedUser = await userManager.FindByEmailAsync(user.Email);

        if (existedUser == null)
        {
            var result = await userManager.CreateAsync(user, userPassword);
            logger.LogIdentityResultError(result, "Unsuccessful admin user creation");
            if (result.Succeeded)
            {
                result = await userManager.AddToRoleAsync(user, Roles.Admin);
                logger.LogIdentityResultError(result, "Failed administrator role assignment");
            }
        }
    }
}

public static class IdentityResultExtensions
{
    public static void LogIdentityResultError(this ILogger logger, IdentityResult result, string title)
    {
        if (result.Succeeded) return;
        var errors = string.Join("\n", result.Errors.Select(e => e.Code + ": " + e.Description));
        logger.LogError("{Title}:\n{Errors}", title, errors);
    }
}