using Microsoft.EntityFrameworkCore;
using RZTask.Server.Models;
using MySql;

namespace RZTask.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Agents> Agents { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<DllFiles> DllFiles { get; set; }
        public DbSet<DllHistory> DllHistory { get; set; }
        public DbSet<TaskAgent> TaskAgent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
