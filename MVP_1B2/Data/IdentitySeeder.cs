using Microsoft.AspNetCore.Identity;
using MVP_1B2.Models;

namespace MVP_1B2.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Administrator",
                "Manager",
                "Employee",
                "Client"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(
            IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            const string email = "admin@test.local";
            const string password = "TestPassword123!";

            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("Administrator"));
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    user,
                    password);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            "; ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, "Administrator"))
            {
                await userManager.AddToRoleAsync(
                    user,
                    "Administrator");
            }
        }
    }
}