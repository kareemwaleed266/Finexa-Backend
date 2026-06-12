using Finexa.Application.Common.Settings;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finexa.Infrastructure.Persistence.Seed
{
    public static class AdminSeeder
    {
        private const string AdminRoleName = "Admin";
        private const string UserRoleName = "User";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<AdminSeedSettings>>()
                .Value;

            if (!settings.Enabled)
                return;

            if (string.IsNullOrWhiteSpace(settings.Email))
                throw new InvalidOperationException("AdminSeed Email is missing");

            if (string.IsNullOrWhiteSpace(settings.Password))
                throw new InvalidOperationException("AdminSeed Password is missing");

            if (string.IsNullOrWhiteSpace(settings.UserName))
                throw new InvalidOperationException("AdminSeed UserName is missing");

            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var context = serviceProvider.GetRequiredService<FinexaDbContext>();

            await EnsureRoleExistsAsync(roleManager, UserRoleName);
            await EnsureRoleExistsAsync(roleManager, AdminRoleName);

            var email = settings.Email.Trim().ToLowerInvariant();
            var userName = settings.UserName.Trim();

            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new AppUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = userName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "AdminSeeder"
                };

                var createResult = await userManager.CreateAsync(
                    admin,
                    settings.Password);

                EnsureSucceeded(createResult, "Failed to create seed admin");
            }
            else
            {
                var changed = false;

                if (!admin.EmailConfirmed)
                {
                    admin.EmailConfirmed = true;
                    changed = true;
                }

                if (!admin.IsActive)
                {
                    admin.IsActive = true;
                    admin.DeactivatedAt = null;
                    admin.DeactivationReason = null;
                    changed = true;
                }

                if (!string.Equals(admin.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    admin.UserName = userName;
                    changed = true;
                }

                if (changed)
                {
                    var updateResult = await userManager.UpdateAsync(admin);
                    EnsureSucceeded(updateResult, "Failed to update seed admin");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, AdminRoleName))
            {
                var addAdminRoleResult = await userManager.AddToRoleAsync(admin, AdminRoleName);
                EnsureSucceeded(addAdminRoleResult, "Failed to add Admin role to seed admin");
            }

            if (!await userManager.IsInRoleAsync(admin, UserRoleName))
            {
                var addUserRoleResult = await userManager.AddToRoleAsync(admin, UserRoleName);
                EnsureSucceeded(addUserRoleResult, "Failed to add User role to seed admin");
            }

            var hasBalance = await context.UserBalances
                .AnyAsync(b => b.AppUserId == admin.Id);

            if (!hasBalance)
            {
                await context.UserBalances.AddAsync(new UserBalance
                {
                    Id = Guid.NewGuid(),
                    AppUserId = admin.Id,
                    TotalIncome = 0,
                    TotalExpense = 0,
                    TotalBalance = 0,
                    CreatedBy = "AdminSeeder"
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureRoleExistsAsync(
            RoleManager<AppRole> roleManager,
            string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                return;

            var result = await roleManager.CreateAsync(new AppRole
            {
                Name = roleName
            });

            EnsureSucceeded(result, $"Failed to create role: {roleName}");
        }

        private static void EnsureSucceeded(IdentityResult result, string message)
        {
            if (result.Succeeded)
                return;

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            throw new InvalidOperationException($"{message}: {errors}");
        }
    }
}