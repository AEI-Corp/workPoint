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

    // Publish the event to RabbitMQ for the assyncronym process.
    public async Task SendWebhookAsync(string eventType, object payload)
    {
        try
        {
            // Serialize the payload
            var payloadJson = JsonSerializer.Serialize(payload);

            // To create the message:
            var message = new WebhookMessage
            {
                EventType = eventType,
                PayloadJson = payloadJson,
                CreatedAt = DateTime.UtcNow
            };

            // Publish RabbitMQ
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
    
