using Garage_3._0.Models;
using Microsoft.AspNetCore.Identity;

namespace Garage_3._0.Data
{
    public class SeedData
    {
        private static ApplicationDbContext _context = default!;
        private static RoleManager<IdentityRole> _roleManager = default!;
        private static UserManager<ApplicationUser> _userManager = default!;
        public static async Task Init(ApplicationDbContext context, IServiceProvider services)
        {
            _context = context;

            if (_context.Roles.Any()) return;

            _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleNames = new[] { "Member", "Admin" };

            var adminEmail = "admin@garage.se";
            var userEmail1 = "member1@garage.se";
            var userEmail2 = "member2@garage.se";

            await AddRolesAsync(roleNames);

            var admin = await AddAccountAsync(adminEmail, "Admin", "Administrator", "19800101-1234", "Admin123!");
            var user1 = await AddAccountAsync(userEmail1, "Member1", "Andersson", "19900515-1111", "Member123!");
            var user2 = await AddAccountAsync(userEmail2, "Member2", "Svensson", "19850320-2222", "Member123!");

            await AddUserToRoleAsync(admin, "Admin");
            await AddUserToRoleAsync(user1, "Member");
            await AddUserToRoleAsync(user2, "Member");
        }

        private static async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));
            }
        }

        private static async Task<ApplicationUser> AddAccountAsync(string accountEmail, string fName, string lName, string PersonalNumber, string pw)
        {
            var found = await _userManager.FindByEmailAsync(accountEmail);

            if (found != null) return null!;

            var user = new ApplicationUser
            {
                UserName = accountEmail,
                Email = accountEmail,
                FirstName = fName,
                LastName = lName,
                PersonalNumber = PersonalNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, pw);

            if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));

            return user;
        }

        private static async Task AddRolesAsync(string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (await _roleManager.RoleExistsAsync(roleName)) continue;

                var role = new IdentityRole { Name = roleName };
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));

            }
        }
    }
}
