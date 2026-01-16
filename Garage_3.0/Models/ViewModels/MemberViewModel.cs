namespace Garage_3._0.Models.ViewModels
{
    public class MemberViewModel 
    {
        public string Id { get; set; }
        public ApplicationUser User { get; set; }
        public int VehicleCount { get; set; }
        public int ActiveParkingCount { get; set; }
        public decimal TotalCurrentCost { get; set; }
        public bool IsProMember { get; set; }
        public List<string> Roles { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}
