using System.ComponentModel.DataAnnotations;

namespace Garage_3._0.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; }
        [Required]
        [StringLength(10)]
        public string SpotNumber { get; set; } = string.Empty;
        public int Size { get; set; }
        public bool IsTaken { get; set; }
        
        public ICollection<Vehicle> Vehicles { get; set; } = [];
        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
