using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using workpoint.Application.Messages;
using workpoint.Domain.Interfaces;
using workpoint.Domain.Interfaces.Repositories;

namespace workpoint.Infrastructure.Messaging;

// Consumer que procesa mensajes de webhook desde RabbitMQ
public class WebhookConsumer : BackgroundService
{
    private readonly ILogger<WebhookConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "webhook.events";

    public WebhookConsumer(
        ILogger<WebhookConsumer> logger,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebhookConsumer iniciado");

        try
        {
            // Obtener configuración de RabbitMQ
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _logger.LogInformation("Esperando mensajes en cola {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<WebhookMessage>(json);

                    if (message != null)
                    {
                        _logger.LogInformation("Mensaje recibido: {EventType}", message.EventType);
                        await ProcessWebhookAsync(message);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensaje");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en WebhookConsumer");
        }
    }

    // Procesa el mensaje y envía webhooks a las URLs suscritas
    private async Task ProcessWebhookAsync(WebhookMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWebhookSubscriptionRepository>();

        // Obtener suscripciones activas
        var subscriptions = await repository.GetActiveByEventTypeAsync(message.EventType);

        if (!subscriptions.Any())
        {
            _logger.LogInformation("No hay suscripciones para {EventType}", message.EventType);
            return;
        }

        var httpClient = _httpClientFactory.CreateClient();

        foreach (var subscription in subscriptions)
        {
            try
            {
                _logger.LogInformation("Enviando webhook a {Url}", subscription.Url);

                // Determinar el formato según la URL
                string json;
                if (subscription.Url.Contains("discord.com"))
                {
                    // Formato Discord
                    json = CreateDiscordPayload(message);
                }
                else
                {
                    // Formato estándar para otros webhooks
                    json = CreateStandardPayload(message);
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(subscription.Url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Webhook enviado exitosamente a {Url}", subscription.Url);
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ Webhook falló. URL: {Url}, Status: {Status}, Response: {Response}",
                        subscription.Url, response.StatusCode, responseBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando webhook a {Url}", subscription.Url);
            }
        }
    }

    // Crea el payload en formato Discord
   // Crea el payload en formato Discord
private string CreateDiscordPayload(WebhookMessage message)
{
    try
    {
        var data = JsonSerializer.Deserialize<JsonElement>(message.PayloadJson);
        
        // Construir campos de forma segura
        var fields = new List<object>
        {
            new { name = "Tipo de Evento", value = message.EventType, inline = true },
            new { name = "Fecha", value = message.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), inline = true }
        };

        // Agregar campos específicos según el tipo de evento
        if (message.EventType.Contains("error") || message.EventType.Contains("failed"))
        {
            // Para eventos de error
            if (data.TryGetProperty("errorType", out var errorType))
                fields.Add(new { name = "Tipo de Error", value = errorType.GetString() ?? "N/A", inline = true });
            
            if (data.TryGetProperty("message", out var errorMessage))
                fields.Add(new { name = "Mensaje", value = TruncateString(errorMessage.GetString() ?? "N/A", 1024), inline = false });
            
            if (data.TryGetProperty("endpoint", out var endpoint))
                fields.Add(new { name = "Endpoint", value = endpoint.GetString() ?? "N/A", inline = true });
            
            if (data.TryGetProperty("statusCode", out var statusCode))
                fields.Add(new { name = "Status Code", value = statusCode.GetInt32().ToString(), inline = true });
        }
        else
        {
            // Para eventos normales (booking.created, etc.)
            var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            fields.Add(new { name = "Detalles", value = $"```json\n{TruncateString(jsonString, 1000)}\n```", inline = false });
        }

        var embed = new
        {
            title = GetEventTitle(message.EventType),
            description = GetEventDescription(message.EventType),
            color = GetEventColor(message.EventType),
            fields = fields.ToArray(),
            timestamp = message.CreatedAt.ToString("o")
        };

        var payload = new
        {
            embeds = new[] { embed }
        };

        return JsonSerializer.Serialize(payload);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creando payload de Discord");
        
        // Payload de fallback simple
        return JsonSerializer.Serialize(new
        {
            content = $"**{GetEventTitle(message.EventType)}**\nError procesando evento. Ver logs del servidor."
        });
    }
}

// Trunca strings largos para Discord (límite de caracteres)
private string TruncateString(string value, int maxLength)
{
    if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        return value;
    
    return value.Substring(0, maxLength - 3) + "...";
}

// Obtiene la descripción según el tipo de evento
private string GetEventDescription(string eventType)
{
    return eventType switch
    {
        "booking.created" => "Una nueva reserva ha sido creada en el sistema",
        "booking.updated" => "Una reserva existente ha sido actualizada",
        "booking.cancelled" => "Una reserva ha sido cancelada",
        "error.occurred" => "Se ha producido un error crítico en el sistema",
        "validation.failed" => "Ha fallado la validación de datos",
        "resource.not_found" => "No se encontró el recurso solicitado",
        "business.logic.error" => "Error en la lógica de negocio",
        _ => "Se ha registrado un nuevo evento"
    };
}

    // Crea el payload en formato estándar
    private string CreateStandardPayload(WebhookMessage message)
    {
        var webhookPayload = new
        {
            eventType = message.EventType,
            timestamp = message.CreatedAt,
            data = JsonSerializer.Deserialize<object>(message.PayloadJson)
        };

        return JsonSerializer.Serialize(webhookPayload);
    }

    // Obtiene el título según el tipo de evento
    private string GetEventTitle(string eventType)
    {
        return eventType switch
        {
            "booking.created" => "🎉 Nueva Reserva Creada",
            "booking.updated" => "📝 Reserva Actualizada",
            "booking.cancelled" => "❌ Reserva Cancelada",
            "error.occurred" => "🔴 Error Crítico",
            "validation.failed" => "⚠️ Validación Fallida",
            "resource.not_found" => "🔍 Recurso No Encontrado",
            "business.logic.error" => "⚠️ Error de Negocio",
            _ => "📢 Nuevo Evento"
        };
    }

    // Obtiene el color según el tipo de evento
    private int GetEventColor(string eventType)
    {
        return eventType switch
        {
            "booking.created" => 0x00FF00,      // Verde
            "booking.updated" => 0x0099FF,      // Azul
            "booking.cancelled" => 0xFF0000,    // Rojo
            "error.occurred" => 0xFF0000,       // Rojo intenso
            "validation.failed" => 0xFFCC00,    // Amarillo
            "resource.not_found" => 0xFF6600,   // Naranja
            "business.logic.error" => 0xFF9900, // Naranja claro
            _ => 0x808080                       // Gris
        };
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}