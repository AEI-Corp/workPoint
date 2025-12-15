using Microsoft.AspNetCore.Mvc;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces;
using workpoint.Domain.Interfaces.Repositories;

namespace workpoint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookSubscriptionController : ControllerBase
{
    private readonly IWebhookSubscriptionRepository _repository;
    private readonly ILogger<WebhookSubscriptionController> _logger;

    public WebhookSubscriptionController(
        IWebhookSubscriptionRepository repository,
        ILogger<WebhookSubscriptionController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // Obtener todas las suscripciones
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var subscriptions = await _repository.GetAllAsync();
        return Ok(subscriptions);
    }

    // Obtener una suscripción por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var subscription = await _repository.GetByIdAsync(id);
        
        if (subscription == null)
            return NotFound(new { message = "Suscripción no encontrada" });
        
        return Ok(subscription);
    }

    // Crear una nueva suscripción
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWebhookDto dto)
    {
        // Validar el tipo de evento
        var validEventTypes = new[] { "booking.created", "booking.updated", "error.occurred", "validation.failed" };
        
        if (!validEventTypes.Contains(dto.EventType))
        {
            return BadRequest(new 
            { 
                message = $"Tipo de evento inválido. Valores válidos: {string.Join(", ", validEventTypes)}" 
            });
        }

        var subscription = new WebhookSubscription
        {
            Url = dto.Url,
            EventType = dto.EventType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(subscription);
        
        _logger.LogInformation("Nueva suscripción creada: {Url} para evento {EventType}", 
            subscription.Url, subscription.EventType);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // Actualizar una suscripción (activar/desactivar)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWebhookDto dto)
    {
        var subscription = await _repository.GetByIdAsync(id);
        
        if (subscription == null)
            return NotFound(new { message = "Suscripción no encontrada" });

        subscription.IsActive = dto.IsActive;
        
        await _repository.UpdateAsync(subscription);
        
        _logger.LogInformation("Suscripción {Id} actualizada. IsActive: {IsActive}", id, dto.IsActive);

        return Ok(subscription);
    }

    // Eliminar una suscripción
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var subscription = await _repository.GetByIdAsync(id);
        
        if (subscription == null)
            return NotFound(new { message = "Suscripción no encontrada" });

        await _repository.DeleteAsync(id);
        
        _logger.LogInformation("Suscripción {Id} eliminada", id);

        return NoContent();
    }
}

// DTOs
public record CreateWebhookDto(string Url, string EventType);
public record UpdateWebhookDto(bool IsActive);