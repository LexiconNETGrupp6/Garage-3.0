namespace Garage_3._0.Models.ViewModels
{
    public class VehicleDetailsViewModel
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public VehicleType? VehicleType { get; set; } = default!;
        public string Color { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int NumberOfWheels { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string ParkingDuration { get {
                var ts = DateTime.Now - ArrivalTime;
                field = string.Empty;
                // Formats for: "X day(s) X hour(s) X minute(s) X second(s)"
                if (ts.Days > 0)
                    field += ts.Days == 1 ? "1 day " : $"{ts.Days} days ";
                if (ts.Hours > 0)
                    field += ts.Hours == 1 ? "1 hour " : $"{ts.Hours} hours ";
                if (ts.Minutes > 0)
                    field += ts.Minutes == 1 ? "1 minute " : $"{ts.Minutes} minutes ";
                if (ts.Hours == 0 && ts.Seconds > 0)
                    field += ts.Seconds == 1 ? "1 second " : $"{ts.Seconds} seconds ";
                return field;
            } } = string.Empty;
        public string? OwnerName { get; set; }
        public string? OwnerEmail { get; set; }
        public IEnumerable<ParkingSpot> ParkingSpots { get; set; } = [];
    }
}
