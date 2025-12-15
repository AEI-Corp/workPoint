using Microsoft.EntityFrameworkCore;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Extensions;

namespace workpoint.Infrastructure.Repositories;

// Repositorio para operaciones de WebhookSubscription
public class WebhookSubscriptionRepository : IWebhookSubscriptionRepository
{
    private readonly AppDbContext _context;

    public WebhookSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    // Obtiene todas las suscripciones
    public async Task<IEnumerable<WebhookSubscription>> GetAllAsync()
    {
        return await _context.WebhookSubscriptions
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    // Obtiene una suscripción por ID
    public async Task<WebhookSubscription?> GetByIdAsync(int id)
    {
        return await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    // Obtiene suscripciones activas para un tipo de evento
    public async Task<IEnumerable<WebhookSubscription>> GetActiveByEventTypeAsync(string eventType)
    {
        return await _context.WebhookSubscriptions
            .Where(w => w.EventType == eventType && w.IsActive)
            .ToListAsync();
    }

    // Crea una nueva suscripción
    public async Task<WebhookSubscription> AddAsync(WebhookSubscription subscription)
    {
        await _context.WebhookSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    // Actualiza una suscripción existente
    public async Task UpdateAsync(WebhookSubscription subscription)
    {
        _context.WebhookSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    // Elimina una suscripción por ID
    public async Task DeleteAsync(int id)
    {
        var subscription = await GetByIdAsync(id);
        if (subscription != null)
        {
            _context.WebhookSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
    }
}