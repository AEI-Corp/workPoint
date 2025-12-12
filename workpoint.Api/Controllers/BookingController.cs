using Microsoft.AspNetCore.Mvc;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using workpoint.Domain.Entities;

namespace workpoint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<BookingListItemDto>>> GetAll()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("spaces/{spaceId}/availability")]
    public async Task<ActionResult<DailyAvailabilityDto>> GetAvailability(
        int spaceId,
        [FromQuery] DateTime date,
        [FromQuery] int slotMinutes = 60)
    {
        var result = await _bookingService.GetDailyAvailabilityAsync(spaceId, date, slotMinutes);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var id = await _bookingService.CreateBookingAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingDto dto)
    {
        await _bookingService.UpdateBookingAsync(id, dto);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingListItemDto>> GetById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
            return NotFound();

        return Ok(booking);
        
    }

    [HttpDelete("{id}/cancel")]
    public async Task<ActionResult> Cancel(int id)
    {
        await _bookingService.CancelBookingAsync(id);
        return NoContent();
    }
}
