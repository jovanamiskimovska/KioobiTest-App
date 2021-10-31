using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.Password)
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.FirstName)
                .HasMaxLength(50)
                .IsRequired();
            
            modelBuilder.Entity<User>()
                .Property(x => x.LastName)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
