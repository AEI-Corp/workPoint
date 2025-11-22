using Microsoft.EntityFrameworkCore;
using workpoint.Domain.Entities;

namespace workpoint.Infrastructure.Extensions;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
            user.HasIndex(u => u.Email)
            .IsUnique();
            user.HasIndex(u => u.NumDocument)
            .IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
}