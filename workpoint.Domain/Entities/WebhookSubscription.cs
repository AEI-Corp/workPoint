namespace workpoint.Domain.Entities;

// Stores the URLs that will receive event notifications
public class WebhookSubscription
{
    // ID unique
    public int Id { get; set; }
    
    // URL that receives notifications
    public string Url { get; set; } = string.Empty;
    
    // Event type: "booking.created", "booking.updated", "error.occurred"
    public string EventType { get; set; } = string.Empty;
    
    // Shows if is active.
    public bool IsActive { get; set; } = true;
    
    // Creation date
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}