namespace workpoint.Domain.Entities;

// Almacena las URLs que recibirán notificaciones de eventos
public class WebhookSubscription
{
    // ID único
    public int Id { get; set; }
    
    // URL que recibirá las notificaciones
    public string Url { get; set; } = string.Empty;
    
    // Tipo de evento: "booking.created", "booking.updated", "error.occurred"
    public string EventType { get; set; } = string.Empty;
    
    // Indica si está activa
    public bool IsActive { get; set; } = true;
    
    // Fecha de creación
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}