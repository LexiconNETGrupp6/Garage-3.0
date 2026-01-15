using Garage_3._0.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Garage_3._0.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
    {
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PersonalNumber)
                .IsUnique();

            builder.Entity<Vehicle>()
                .HasIndex(v => v.LicenseNumber)
                .IsUnique();

            builder.Entity<VehicleType>().HasData(
                new VehicleType { Id = 1, Name = "Car", Size = 1 },
                new VehicleType { Id = 2, Name = "Motorcycle", Size = 1 },
                new VehicleType { Id = 3, Name = "Bus", Size = 3 },
                new VehicleType { Id = 4, Name = "Boat", Size = 4 }
    );
        }
    }
}