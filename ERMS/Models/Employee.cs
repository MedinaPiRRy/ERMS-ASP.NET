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
    }
}
