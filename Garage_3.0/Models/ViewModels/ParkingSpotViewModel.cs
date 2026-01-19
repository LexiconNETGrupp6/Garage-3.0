namespace Garage_3._0.Models.ViewModels
{
    public class ParkingSpotViewModel
    {
        public int Number { get; set; }
        public int VehicleId { get; set; }
        public IEnumerable<string> Vehicles { get; set; } = [];
    }
}
