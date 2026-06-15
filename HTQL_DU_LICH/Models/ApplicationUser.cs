using Microsoft.AspNetCore.Identity;

namespace HTQL_DU_LICH.Models

{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; } 

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }
    }
}