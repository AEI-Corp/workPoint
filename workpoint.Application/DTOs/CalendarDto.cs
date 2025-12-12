namespace workpoint.Application.DTOs;

public class TimeSlotDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = "free"; // "free" | "reserved"
}

public partial class DailyAvailabilityDto
{
    public int SpaceId { get; set; }
    public DateTime Date { get; set; }
    public int SlotMinutes { get; set; }
    public List<TimeSlotDto> Slots { get; set; } = new();
}

public class CreateBookingDto
{
    public int SpaceId { get; set; }
    public int UserId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBookingDto
{
    public int SpaceId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
