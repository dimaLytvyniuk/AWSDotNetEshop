using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace EventBusSns;

public interface IAmazonQueueBuilder
{
    IAmazonQueueBuilder AddEventHandler<TMessage, TMessageReceiver>()
        where TMessage : IntegrationEvent
        where TMessageReceiver : class, IIntegrationEventHandler<TMessage>;
}
