using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERMS.Models;

namespace ERMS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
