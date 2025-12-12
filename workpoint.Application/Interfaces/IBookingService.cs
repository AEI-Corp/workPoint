using workpoint.Application.DTOs;

namespace workpoint.Application.Interfaces;

public interface IBookingService
{
    Task<DailyAvailabilityDto> GetDailyAvailabilityAsync(int spaceId, DateTime date, int slotMinutes);
    Task<int> CreateBookingAsync(CreateBookingDto dto);
    Task CancelBookingAsync(int id);
    Task<List<BookingListItemDto>> GetAllBookingsAsync();
    Task<BookingListItemDto?> GetBookingByIdAsync(int id);
    Task UpdateBookingAsync(int id, UpdateBookingDto dto);
}
