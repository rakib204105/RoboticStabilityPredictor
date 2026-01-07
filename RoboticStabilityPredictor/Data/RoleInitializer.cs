using Microsoft.AspNetCore.Identity;

namespace RoboticStabilityPredictor.Data
{
    public static class RoleInitializer
    {
        private static readonly string[] Roles = new[] { "Admin", "RegisteredUser", "Guest" };

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
