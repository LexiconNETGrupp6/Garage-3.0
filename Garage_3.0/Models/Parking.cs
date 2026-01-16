namespace Garage_3._0.Models
{
    public class Parking
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public int ParkingSpotId { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.Now;

        public DateTime? CheckOutTime { get; set; }

        public decimal Cost { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Vehicle Vehicle { get; set; } = null!;
        public ParkingSpot ParkingSpot { get; set; } = null!;
        public TimeSpan GetParkingDuration()
        {
            var endTime = CheckOutTime ?? DateTime.Now;
            return endTime - CheckInTime;
        }

        public decimal CalculateCost(decimal hourlyRate = 20m, bool isProMember = false)
        {
            var duration = GetParkingDuration();
            var hours = Math.Ceiling(duration.TotalHours);
            var cost = (decimal)hours * hourlyRate;

            if (isProMember)
            {
                cost *= 0.8m; // 20% discount for Pro members
            }

            return cost;
        }
    }
}
