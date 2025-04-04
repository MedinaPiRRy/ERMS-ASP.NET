using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ERMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
        public string Manager { get; set; }

        public int? ProjectId { get; set; } 
        public Project? Project { get; set; }

        // For security purposes. 
        // This is the ID of the user in the Identity system.
        public string? IdentityUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }

    }
}
