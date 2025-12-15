namespace workpoint.Application.Interfaces;

public interface IWebhookService
{
    Task SendWebhookAsync(string eventType, object payload);
}