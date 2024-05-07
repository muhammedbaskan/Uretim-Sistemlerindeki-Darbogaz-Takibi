using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PANOPA.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Process> Processes { get; set; }
        public DbSet<Delay> Delays { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProcess> UserProcesses { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=panopa.db");
            }
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
        }
    }
}
