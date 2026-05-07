using Microsoft.AspNetCore.Identity;

namespace Exam_Test.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                bool exists = await roleManager.RoleExistsAsync(role);

                if (!exists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}