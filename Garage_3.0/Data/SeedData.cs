
using Bogus;
using Bogus.Extensions.Sweden;
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
        private static Faker _faker = default!;

        public static async Task InitAsync(ApplicationDbContext context, IServiceProvider services)
        {
            _context = context;


            if (_context.Roles.Any()) return;

            _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            _faker = new("sv");

            await AddRolesAsync([RolesNames.Admin, RolesNames.Member]);

            ApplicationUser admin = await CreateUserAsync("admin@garage.se", "Admin", "Adminsson", "19800101-1111", "P@55w.rD!");
            ApplicationUser member = await CreateUserAsync("member@garage.se", "User", "Usersson", "19760101-1111", "P@55w.rD!");

            /*IEnumerable<ApplicationUser> members = */await GenerateUsers(30);
            
            
        }

        private static async Task<IEnumerable<ApplicationUser>> GenerateUsers(int numberOfUsers)
        {
            ICollection<ApplicationUser> members = [];
            for (int i = 0; i < numberOfUsers; i++) {
                members.Add(await CreateUserAsync(
                    email: _faker.Internet.Email(),
                    fName: _faker.Name.FirstName(),
                    lName: _faker.Name.LastName(),
                    personalNumber: _faker.Person.Personnummer(),
                    password: _faker.Internet.Password()
                ));
            }
            return members;
        }

        private static async Task<ApplicationUser> CreateUserAsync(string email, string fName, string lName, string personalNumber, string password)
        {
            ApplicationUser? found = await _userManager.FindByEmailAsync(email);
            if (found is not null) return null!;

            ApplicationUser user = new() {
                UserName = email,
                Email = email,
                FirstName = fName,
                LastName = lName,
                EmailConfirmed = true,
                PersonalNumber = personalNumber,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));
            return user;
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