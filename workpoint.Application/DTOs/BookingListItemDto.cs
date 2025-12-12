namespace workpoint.Application.DTOs;

public class BookingListItemDto
{
    public int Id { get; set; }
    public int SpaceId { get; set; }
    public int? UserId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool Available { get; set; }
}