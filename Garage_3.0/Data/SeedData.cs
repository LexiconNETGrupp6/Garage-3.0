
using Bogus;
using Bogus.Extensions.Sweden;
using Garage_3._0.ConstantValues;
using Garage_3._0.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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


            if (_context.Roles.Any()) return;

            _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            _faker = new("sv");
            _rnd = new(10);

            await AddRolesAsync([RolesNames.Admin, RolesNames.Member]);

            ApplicationUser admin = await CreateUserAsync("admin@garage.se", "Admin", "Adminsson", "198001011111", "P@55w.rD!");
            ApplicationUser member = await CreateUserAsync("member@garage.se", "User", "Usersson", "197601011111", "P@55w.rD!");

            List<ApplicationUser> members = await GenerateUsers(30);

            List<VehicleType> vehicleTypes = await GenerateVehicleTypes();
            _context.VehicleTypes.AddRange(vehicleTypes);


            List<Vehicle> vehicles = await GenerateVehicles(40);
            _context.Vehicles.AddRange(vehicles);


            List<ParkingSpot> parkingSpots = await GenerateParkingSpots(55);
            _context.ParkingSpots.AddRange(parkingSpots);
            
            await _context.SaveChangesAsync();

            await JoinVehiclesAndVehicleTypes();
            await JoinUsersAndVehicles();
            await JoinVehiclesAndParkingSpots();

            await _context.SaveChangesAsync();
        }

        private static async Task JoinVehiclesAndParkingSpots()
        {
            throw new NotImplementedException();
        }

        private static async Task JoinUsersAndVehicles()
        {
            throw new NotImplementedException();
        }

        private static async Task JoinVehiclesAndVehicleTypes()
        {
            List<Vehicle> vehicles = await _context.Vehicles.ToListAsync();
            List<VehicleType> vehicleTypes = await _context.VehicleTypes.ToListAsync();

            int numberOfVehicleTypes = vehicleTypes.Count();
            foreach (var vehicle in vehicles) {
                VehicleType type = vehicleTypes[_rnd.Next(0, numberOfVehicleTypes)];
                vehicle.VehicleType = type;
                vehicle.VehicleTypeId = type.Id;
            }
        }

        private static async Task<List<VehicleType>> GenerateVehicleTypes()
        {
            List<VehicleType> vehicleTypes = [];
            vehicleTypes.Add(new VehicleType { Name = "Boat", Size = 2 });
            vehicleTypes.Add(new VehicleType { Name = "Bus", Size = 3 });
            vehicleTypes.Add(new VehicleType { Name = "Car", Size = 1 });
            vehicleTypes.Add(new VehicleType { Name = "Motorcycle", Size = 1 });
            vehicleTypes.Add(new VehicleType { Name = "Truck", Size = 3 });

            return vehicleTypes;
        }

        private static async Task<List<ParkingSpot>> GenerateParkingSpots(int numberOfSpots)
        {
            List<ParkingSpot> spots = [];
            for (int i = 0; i < numberOfSpots; i++) {
                spots.Add(new() {
                    Size = _rnd.Next(1, 5),
                    IsTaken = false
                });
            }
            return spots;
        }        

        private static async Task<List<Vehicle>> GenerateVehicles(int numberOfVehicles)
        {
            List<Vehicle> vehicles = [];

            // First vehicle using DateTime.Now to get a recent time
            vehicles.Add(new Vehicle {
                LicenseNumber = _faker.Random.Replace("??? ##*"), // regex: /[A-Z]{3} \d{2}[A-Z0-9]/
                ParkedDuration = TimeSpan.FromMinutes(_rnd.Next(601)),
                Model = _faker.Vehicle.Model(),
                Color = _faker.Commerce.Color(),
                NumberOfWheels = _rnd.Next(0, 13),
                ArrivalTime = DateTime.Now,
            });

            for (int i = 0; i < numberOfVehicles-1; i++) {
                vehicles.Add(new Vehicle {
                    LicenseNumber = _faker.Random.Replace("??? ##*"), // regex: /[A-Z]{3} \d{2}[A-Z0-9]/
                    ParkedDuration = TimeSpan.FromMinutes(_rnd.Next(601)),
                    Model = _faker.Vehicle.Model(),
                    Color = _faker.Commerce.Color(),
                    NumberOfWheels = _rnd.Next(0, 13),
                    ArrivalTime = new DateTime(
                        year: _rnd.Next(2024, 2026),
                        month: _rnd.Next(1, 13),
                        day: _rnd.Next(1, 28),
                        hour: _rnd.Next(0, 24),
                        minute: _rnd.Next(0, 60),
                        second: _rnd.Next(0, 60)
                        ),
                });
            }            

            return vehicles;
        }

        private static async Task<List<ApplicationUser>> GenerateUsers(int numberOfUsers)
        {
            List<ApplicationUser> members = [];
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