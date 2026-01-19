namespace Garage_3._0.Models.ViewModels
{
    public class MemberDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public List<Parking> ActiveSessions { get; set; }
        public decimal TotalCurrentCost { get; set; }
        public decimal TotalHistoricalRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalParkings { get; set; }
        public bool IsProMember { get; set; }
        public List<string> Roles { get; set; }
    }
}
