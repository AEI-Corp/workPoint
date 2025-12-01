namespace workpoint.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? SpaceId { get; set; }
    public DateTime StartHour { get; set; }
    public DateTime EndHour { get; set; }
    public bool Available { get; set; }
    
    // Relations
    public User User { get; set; }
    public Space Space { get; set; }
    
}