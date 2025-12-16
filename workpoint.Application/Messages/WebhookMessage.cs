namespace workpoint.Application.Messages;

public class WebhookMessage
{
    // Event type: "booking.created", "booking.updated", "error.occurred"
    public string EventType { get; set; } = string.Empty;
    
    // Event data: (JSON serializes)
    public string PayloadJson { get; set; } = string.Empty;
    
    // Timestamp when it was created the message
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}