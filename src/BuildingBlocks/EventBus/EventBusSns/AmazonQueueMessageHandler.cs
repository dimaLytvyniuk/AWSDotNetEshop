using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventBusSns;

public class AmazonQueueMessageHandler<TMessage> : IAmazonQueueMessageHandler
    where TMessage : IntegrationEvent
{
    private readonly IServiceProvider _serviceProvider;

    public AmazonQueueMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task HandleMessageAsync(string serializedMessage)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AmazonQueueMessageHandler<TMessage>>>();

        try
        {
            logger.LogInformation("Message Received: {message}", serializedMessage);
            var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TMessage>>();

            logger.LogInformation("Start message handling: {MessageType}", typeof(TMessage));
            var message = JsonSerializer.Deserialize<TMessage>(serializedMessage);
            await handler.Handle(message);
            logger.LogInformation("Start message handling: {MessageType}", typeof(TMessage));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during message processing");
        }
    }
}
