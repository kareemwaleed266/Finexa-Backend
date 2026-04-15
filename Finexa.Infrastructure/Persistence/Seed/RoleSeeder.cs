using Finexa.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Finexa.Infrastructure.Persistence.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<AppRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = "User",
                });
            }

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = "Admin"
                });
            }
        }
    }
}