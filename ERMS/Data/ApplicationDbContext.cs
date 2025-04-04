using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERMS.Models;
using System.Reflection.Emit;

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>()
                .HasMany(p=>p.AssignedEmployees)
                .WithOne(e=>e.Project)
                .HasForeignKey(e=>e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Employee>()
            .HasOne(e => e.IdentityUser)
            .WithMany()
            .HasForeignKey(e => e.IdentityUserId)
            .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
