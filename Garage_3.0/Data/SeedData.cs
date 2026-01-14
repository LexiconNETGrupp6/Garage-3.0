
using Garage_3._0.ConstantValues;
using Garage_3._0.Models;
using Microsoft.AspNetCore.Identity;

namespace Garage_3._0.Data
{
    public class SeedData
    {
        private static ApplicationDbContext _context = default!;
        private static RoleManager<IdentityRole> _roleManager = default!;
        private static UserManager<ApplicationUser> _userManager = default!;

        public static async Task InitAsync(ApplicationDbContext context, IServiceProvider services)
        {
            _context = context;


            if (_context.Roles.Any()) return;

            _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await AddRolesAsync([RolesNames.Admin, RolesNames.Member]);
        }

        private static async Task AddRolesAsync(string[] roleNames)
        {
            foreach (var roleName in roleNames) {
                if (await _roleManager.RoleExistsAsync(roleName)) continue;

                IdentityRole role = new() { Name = roleName };
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));
            }
        }
    }
}