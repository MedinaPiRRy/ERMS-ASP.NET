using System.ComponentModel.DataAnnotations;

namespace ERMS.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string Priority { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int EmployeeId { get; set; }
        public Employee AssignedEmployee { get; set; }
    }
}
