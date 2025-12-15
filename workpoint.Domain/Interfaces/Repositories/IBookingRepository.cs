using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using workpoint.Domain.Entities;

namespace workpoint.Domain.Interfaces.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetBySpaceAndDateAsync(int spaceId, DateTime date);
        Task<bool> HasConflictAsync(int spaceId, DateTime start, DateTime end);

        // crud that uses service
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task<Booking?> GetByIdAsync(int id);
        Task<List<Booking>> GetAllAsync();
        Task<bool> HasConflictForUpdateAsync(int bookingId, int spaceId, DateTime start, DateTime end);

    }
}