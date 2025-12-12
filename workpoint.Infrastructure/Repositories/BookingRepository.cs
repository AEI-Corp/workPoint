using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Extensions;

namespace workpoint.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Booking>> GetBySpaceAndDateAsync(int spaceId, DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd   = dayStart.AddDays(1);

            return await _context.Bookings
                .Where(b => b.SpaceId == spaceId
                            && b.Available
                            && b.StartHour < dayEnd
                            && b.EndHour   > dayStart)
                .ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int spaceId, DateTime start, DateTime end)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.SpaceId == spaceId &&
                b.Available &&
                b.StartHour < end &&
                start < b.EndHour);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .OrderByDescending(b => b.StartHour)
                .ToListAsync();
        }

        public async Task<bool> HasConflictForUpdateAsync(int bookingId, int spaceId, DateTime start, DateTime end)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.Id != bookingId &&
                b.SpaceId == spaceId &&
                b.Available &&
                b.StartHour < end &&
                start < b.EndHour);        }
    }
}