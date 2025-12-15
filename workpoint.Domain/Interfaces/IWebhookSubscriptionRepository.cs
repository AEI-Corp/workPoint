using workpoint.Domain.Entities;

namespace workpoint.Domain.Interfaces;

public interface IWebhookSubscriptionRepository
{
    Task<IEnumerable<WebhookSubscription>> GetAllAsync();
    
    // Obtener una suscripción por ID
    Task<WebhookSubscription?> GetByIdAsync(int id);
    
    // Obtener suscripciones activas por tipo de evento
    Task<IEnumerable<WebhookSubscription>> GetActiveByEventTypeAsync(string eventType);
    
    // Crear una nueva suscripción
    Task<WebhookSubscription> AddAsync(WebhookSubscription subscription);
    
    // Actualizar una suscripción existente
    Task UpdateAsync(WebhookSubscription subscription);
    
    // Eliminar una suscripción
    Task DeleteAsync(int id);
}