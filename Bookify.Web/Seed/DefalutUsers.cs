using Microsoft.AspNetCore.Identity;

namespace Bookify.Web.Seed
{
    public class DefalutUsers
    {
        public static async Task SeedUsers(UserManager<ApplicationUsers> userManager)
        {
            ApplicationUsers admin = new()
            {
                UserName = "admin",
                Email = "admin@bookify.com",
                FullName = "Admin",
                EmailConfirmed = true 
            };
            var user = await userManager.FindByNameAsync(admin.UserName);
            if (user is null)
            {
                await userManager.CreateAsync(admin, "Mo@192002");
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}
