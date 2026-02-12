using Microsoft.AspNetCore.Identity;

namespace WebsiteSellLaptop.Models.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
