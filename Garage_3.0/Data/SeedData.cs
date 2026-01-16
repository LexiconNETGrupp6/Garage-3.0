using Bogus;
using Bogus.Extensions.Sweden;
using Garage_3._0.ConstantValues;
using Garage_3._0.Extensions;
using Garage_3._0.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Garage_3._0.Data
{
    public class SeedData
    {
        private static ApplicationDbContext _context = default!;
        private static RoleManager<IdentityRole> _roleManager = default!;
        private static UserManager<ApplicationUser> _userManager = default!;
        private static Faker _faker = default!;
        private static Random _rnd = default!;

        public static async Task InitAsync(ApplicationDbContext context, IServiceProvider services)
        {
            _context = context;

            _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            _faker = new("sv");
            _rnd = new();

            //  Seed VehicleTypes if missing (runs even if roles already exist)
            if (!await _context.VehicleTypes.AnyAsync())
            {
                List<VehicleType> vehicleTypes = await GenerateVehicleTypes();
                _context.AddRange(vehicleTypes);
                await _context.SaveChangesAsync();
            }

            //  Seed Roles/Users ONLY once
            bool hasRoles = await _context.Roles.AnyAsync();
            if (hasRoles) return;

            await AddRolesAsync([RolesNames.Admin, RolesNames.Member]);

            ApplicationUser admin = await CreateUserAsync("admin@garage.se", "Admin", "Adminsson", "19800101-1111", "P@55w.rD!");
            ApplicationUser member = await CreateUserAsync("member@garage.se", "User", "Usersson", "19760101-1111", "P@55w.rD!");

            await AddRoleToUserAsync(admin, RolesNames.Admin);
            await AddRoleToUserAsync(member, RolesNames.Member);

            List<ApplicationUser> members = [admin, member, .. await GenerateUsers(30)];
            foreach (ApplicationUser m in members)
            {
                await AddRoleToUserAsync(m, RolesNames.Member);
            }

            // After users + types exist, create vehicles + parking spots
            List<Vehicle> vehicles = await GenerateVehicles(35);
            _context.AddRange(vehicles);

            List<ParkingSpot> parkingSpots = await GenerateParkingSpots(55);
            _context.AddRange(parkingSpots);

            await _context.SaveChangesAsync();

            await JoinVehiclesAndParkingSpots();
        }


        private static async Task JoinVehiclesAndParkingSpots()
        {
            List<Vehicle> vehicles = await _context.Vehicles.ToListAsync();
            List<ParkingSpot> parkingSpots = await _context.ParkingSpots.OrderBy(p => p.Size).ToListAsync();

            foreach (var vehicle in vehicles) {
                bool found = false;
                // Loop through the parking spots...
                for (int i = 0; i < parkingSpots.Count; i++) {
                    // ...until that has enough space left is found
                    if (!parkingSpots[i].IsTaken && await parkingSpots[i].GetRemaingSpace(_context) >= vehicle.VehicleType!.Size) {
                        parkingSpots[i].Vehicles.Add(vehicle);
                        vehicle.ParkingSpots.Add(parkingSpots[i]);

                        if (await parkingSpots[i].GetRemaingSpace(_context) == 0)
                            parkingSpots[i].IsTaken = true;

                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // This point should only be reached if a vehicle didn't get assigned a spot
                    // Since vehicles need a spot, just throw an error and re-run the SeedData (probably not good practice)
                    throw new Exception($"Vehicle {vehicle.Id} failed to find a space");
                }
            }

            // NEEDED: updates the lists so that the database knows them
            _context.UpdateRange(vehicles);
            _context.UpdateRange(parkingSpots);
            await _context.SaveChangesAsync();
        }

        private static async Task<List<VehicleType>> GenerateVehicleTypes()
        {
            List<VehicleType> vehicleTypes = [];
            vehicleTypes.Add(new VehicleType { Name = "Boat", Size = 3 });
            vehicleTypes.Add(new VehicleType { Name = "Bus", Size = 4 });
            vehicleTypes.Add(new VehicleType { Name = "Car", Size = 2 });
            vehicleTypes.Add(new VehicleType { Name = "Motorcycle", Size = 1 });
            vehicleTypes.Add(new VehicleType { Name = "Truck", Size = 4 });

            return vehicleTypes;
        }

        private static async Task<List<ParkingSpot>> GenerateParkingSpots(int numberOfSpots)
        {
            List<ParkingSpot> spots = [];
            for (int i = 0; i < numberOfSpots; i++) {
                spots.Add(new() {
                    Size = _rnd.Next(1, 6),
                    IsTaken = false
                });
            }
            return spots;
        }

        private static async Task<List<Vehicle>> GenerateVehicles(int numberOfVehicles)
        {
            List<ApplicationUser> members = await _context.Users.ToListAsync();
            List<VehicleType> vehicleTypes = await _context.VehicleTypes.ToListAsync();

            List<Vehicle> vehicles = [];

            int vehicleTypeCount = vehicleTypes.Count;
            int memberCount = members.Count;

            ApplicationUser owner = members[0];
            VehicleType type = vehicleTypes[_rnd.Next(0, vehicleTypeCount)];

            // First vehicle using DateTime.Now to get a recent time
            vehicles.Add(new Vehicle {
                LicenseNumber = _faker.Random.Replace("??? ##*"), // regex: /[A-Z]{3} \d{2}[A-Z0-9]/
                ParkedDuration = TimeSpan.FromMinutes(_rnd.Next(601)),
                Model = _faker.Vehicle.Model(),
                Color = _faker.Commerce.Color(),
                NumberOfWheels = _rnd.Next(0, 13),
                ArrivalTime = DateTime.Now,
                Owner = owner,
                OwnerId = owner.Id,
                VehicleType = type,
                VehicleTypeId = type.Id,
            });

            for (int i = 1; i < numberOfVehicles; i++) {
                // Takes the next member in line (loops at end)
                // and a random vehicle type to assign to the vehicle
                owner = members[i % memberCount];
                type = vehicleTypes[_rnd.Next(0, vehicleTypeCount)];
                vehicles.Add(new Vehicle {
                    LicenseNumber = _faker.Random.Replace("??? ##*"), // regex: /[A-Z]{3} \d{2}[A-Z0-9]/
                    ParkedDuration = TimeSpan.FromMinutes(_rnd.Next(601)),
                    Model = _faker.Vehicle.Model(),
                    Color = _faker.Commerce.Color(),
                    NumberOfWheels = _rnd.Next(0, 13),
                    // Arrival time anytime during 2024 or 2025
                    // I don't want to handle future cases (e.g. 2026/4/12) rn
                    // so just one manual DateTime.Now before the loop
                    ArrivalTime = new DateTime(
                        year: _rnd.Next(2024, 2026),
                        month: _rnd.Next(1, 13),
                        day: _rnd.Next(1, 28),
                        hour: _rnd.Next(0, 24),
                        minute: _rnd.Next(0, 60),
                        second: _rnd.Next(0, 60)
                        ),
                    Owner = owner,
                    OwnerId = owner.Id,
                    VehicleType = type,
                    VehicleTypeId = type.Id,
                });
            }

            return vehicles;
        }

        private static async Task<List<ApplicationUser>> GenerateUsers(int numberOfUsers)
        {
            List<ApplicationUser> members = [];
            List<string> personNumberList = [];

            // Move DateTime.Now back by a random number of days, months, and years
            // Add it to the list if it's unique
            while (personNumberList.Count < numberOfUsers) {
                DateTime date = DateTime.Now;
                date = date.AddDays(_rnd.Next(-20, -1))
                    .AddMonths(_rnd.Next(-30, 0))
                    .AddYears(_rnd.Next(-80, -15));
                string personNumber = _faker.Random.Replace($"{date.ToString("yyyyMMdd")}-####");
                if (!personNumberList.Contains(personNumber))
                    personNumberList.Add(personNumber);
            }

            for (int i = 0; i < numberOfUsers; i++) {
                members.Add(await CreateUserAsync(
                    email: _faker.Internet.Email(),
                    fName: _faker.Name.FirstName(),
                    lName: _faker.Name.LastName(),
                    personalNumber: personNumberList[i],
                    // Faker.Internet.Password can fail the password requirements
                    // so, just uses a simple one that works
                    password: "Aa111!"
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

        private static async Task AddRoleToUserAsync(ApplicationUser user, string role)
        {
            if (!await _userManager.IsInRoleAsync(user, role)) {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));
            }
        }
    }
}