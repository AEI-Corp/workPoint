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
        // To have each Email & NumDocument as Unique registers:
        var user = modelBuilder.Entity<User>();
            user.HasIndex(u => u.Email)
            .IsUnique();
            user.HasIndex(u => u.NumDocument)
            .IsUnique();

            // To have a list of photos in Spaces:
            modelBuilder.Entity<Space>()
                .HasMany(s => s.Photos)
                .WithOne(i => i.Space)
                .HasForeignKey(i => i.SpaceId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Booking 1:1 Payment
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Categorie> Categories { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Space> Spaces { get; set; }
    public DbSet<User> Users { get; set; }
    
}