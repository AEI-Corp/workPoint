namespace workpoint.Application.Messages;

public class WebhookMessage
{
    // Tipo de evento: "booking.created", "booking.updated", "error.occurred"
    public string EventType { get; set; } = string.Empty;
    
    // Datos del evento (JSON serializado)
    public string PayloadJson { get; set; } = string.Empty;
    
    // Timestamp de cuando se creó el mensaje
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}