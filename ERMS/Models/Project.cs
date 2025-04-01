using System.ComponentModel.DataAnnotations;

namespace ERMS.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string Status { get; set; }

        public ICollection<Employee> AssignedEmployees { get; set; }
    }
}
