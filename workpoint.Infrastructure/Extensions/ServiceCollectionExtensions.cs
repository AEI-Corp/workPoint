using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Repositories;

namespace workpoint.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string conn = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                conn,
                ServerVersion.AutoDetect(conn)
            )
        );
        
        services.AddScoped<IBookingRepository, BookingRepository>();
        return services;
    }
}