using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using System.Threading.Tasks;

namespace EventBusSns;

public interface IAmazonQueueEventBus
{
    Task Publish(IntegrationEvent @event, string topicName);
}
