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
        }
    }
}