using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using workpoint.Application.Interfaces;

namespace workpoint.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        
        _hostName = configuration["RabbitMQ:Host"] ?? "localhost";
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        _userName = configuration["RabbitMQ:Username"] ?? "guest";
        _password = configuration["RabbitMQ:Password"] ?? "guest";
    }

    // Inicializa la conexión solo cuando se necesita
    private async Task EnsureConnectionAsync()
    {
        if (_connection != null && _channel != null)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        
        _logger.LogInformation("Conexión a RabbitMQ establecida");
    }

    public void Publish<T>(string queueName, T message)
    {
        PublishAsync(queueName, message).GetAwaiter().GetResult();
    }

    private async Task PublishAsync<T>(string queueName, T message)
    {
        try
        {
            await EnsureConnectionAsync();

            // Declarar la cola
            await _channel!.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Serializar mensaje
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // Publicar
            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body
            );

            _logger.LogInformation("Mensaje publicado en cola {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando mensaje en cola {QueueName}", queueName);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}