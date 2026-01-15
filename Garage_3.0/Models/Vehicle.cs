namespace Garage_3._0.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public TimeSpan ParkedDuration { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int NumberOfWheels { get; set; }
        public DateTime ArrivalTime { get; set; }

        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }
        public int VehicleTypeId { get; set; }
        public VehicleType? VehicleType { get; set; }
        public ICollection<ParkingSpot> ParkingSpots { get; set; } = [];
        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
