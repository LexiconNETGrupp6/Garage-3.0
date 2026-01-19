namespace Garage_3._0.Models.ViewModels
{
    public class ParkingSpotViewModel
    {
        public string Number { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public IEnumerable<string> Vehicles { get; set; } = [];
    }
}
