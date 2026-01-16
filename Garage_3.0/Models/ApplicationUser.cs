using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Garage_3._0.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty;
        public bool IsProMember { get; set; }
        [Required]
        [StringLength(13)] // YYYYMMDD-XXXX format
        public string PersonalNumber { get; set; } = string.Empty;        

        public DateTime? ProMembershipExpiry { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = [];        
    }
}
