using System.Text.Json;
using Microsoft.Extensions.Logging;
using workpoint.Application.Interfaces;
using workpoint.Application.Messages;

namespace workpoint.Application.Services;

public class WebhookService : IWebhookService
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<WebhookService> _logger;
    private const string QueueName = "webhook.events";

    public WebhookService(
        IMessagePublisher messagePublisher,
        ILogger<WebhookService> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    // Publica el evento a RabbitMQ para procesamiento asíncrono
    public async Task SendWebhookAsync(string eventType, object payload)
    {
        try
        {
            // Serializar el payload
            var payloadJson = JsonSerializer.Serialize(payload);

            // Crear mensaje
            var message = new WebhookMessage
            {
                EventType = eventType,
                PayloadJson = payloadJson,
                CreatedAt = DateTime.UtcNow
            };

            // Publicar a RabbitMQ
            _messagePublisher.Publish(QueueName, message);

            _logger.LogInformation("Evento {EventType} publicado a RabbitMQ", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando evento {EventType} a RabbitMQ", eventType);
            throw;
        }

        await Task.CompletedTask;
    }
}
    
