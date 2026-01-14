using Microsoft.AspNetCore.Identity;

namespace Garage_3._0.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty;

        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
