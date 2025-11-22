using Microsoft.EntityFrameworkCore;
using workpoint.Domain.Entities;

namespace workpoint.Infrastructure.Extensions;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
        
    }
}