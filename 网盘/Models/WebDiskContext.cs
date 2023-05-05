
// Models/WebDiskContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace 网盘.Models
{
    // A class that represents the database context for the web disk
    public class WebDiskContext : IdentityDbContext<User, Role, string>
    {
        // A constructor that takes the database options
        public WebDiskContext(DbContextOptions<WebDiskContext> options) : base(options)
        {
        }

        // A property that represents the files table
        public DbSet<File> Files { get; set; }

        // A method that configures the database model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationships between the entities
            modelBuilder.Entity<File>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<File>()
                .HasOne(f => f.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(f => f.ParentId);
        }
    }
}
