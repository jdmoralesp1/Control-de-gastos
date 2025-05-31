using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace PruebaTecnicaMVC.AccesoDatos.Seed;
public static class SeedUserData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roleExist = await roleManager.RoleExistsAsync("Administrator");
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole("Administrator"));
        }

        await CreateUsers("admin", "admin", "Administrator", userManager, "e16bfd7a-a24b-40c6-92d4-dbddb739fb47");
    }

    private static async Task CreateUsers(string email, string password, string rol, UserManager<IdentityUser> userManager, string id)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            var adminUser = new IdentityUser
            {
                Id = id,
                UserName = email,
                Email = email
            };

            var createAdminUser = await userManager.CreateAsync(adminUser, password);
            if (createAdminUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, rol);
            }
        }
    }
}
