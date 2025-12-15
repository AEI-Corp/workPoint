using workpoint.Domain.Entities;

namespace workpoint.Domain.Interfaces;

public interface IWebhookSubscriptionRepository
{
    Task<IEnumerable<WebhookSubscription>> GetAllAsync();
    
    // Get a subscription by ID
    Task<WebhookSubscription?> GetByIdAsync(int id);
    
    // Obtain active subscriptions by event type
    Task<IEnumerable<WebhookSubscription>> GetActiveByEventTypeAsync(string eventType);
    
    // Create a new subscription
    Task<WebhookSubscription> AddAsync(WebhookSubscription subscription);
    
    // Update an existing subscription
    Task UpdateAsync(WebhookSubscription subscription);
    
    // Delete a subscription
    Task DeleteAsync(int id);
}