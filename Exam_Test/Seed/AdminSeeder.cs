using Microsoft.AspNetCore.Identity;

namespace Exam_Test.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string adminEmail = "admin@test.com";
            string adminPassword = "Admin@123";

            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                var newUser = new IdentityUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Admin");
                }
            }
        }
    }
}