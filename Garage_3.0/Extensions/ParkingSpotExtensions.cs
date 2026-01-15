using Garage_3._0.Data;
using Garage_3._0.Models;
using Microsoft.EntityFrameworkCore;

namespace Garage_3._0.Extensions
{
    public static class ParkingSpotExtensions
    {
        private static ApplicationDbContext _context = default!;
        public static async Task<int> GetRemaingSpace(this ParkingSpot spot, ApplicationDbContext context)
        {
            int used = 0;

            foreach (Vehicle vehicle in spot.Vehicles) {
                int current = 0;
                if (vehicle.VehicleType is null) {
                    VehicleType? tempVehicle = await _context.VehicleTypes.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
                    current = tempVehicle!.Size;
                } else
                    current = vehicle.VehicleType.Size;

                used += current;
            }

            return spot.Size - used;
        }
    }
}
