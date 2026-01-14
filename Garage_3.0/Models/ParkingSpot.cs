namespace Garage_3._0.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public bool IsTaken { get; set; }
        
        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
