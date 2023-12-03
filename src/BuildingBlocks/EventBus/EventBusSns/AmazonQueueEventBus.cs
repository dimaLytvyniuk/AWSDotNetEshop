using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBusSns;

public class AmazonQueueEventBus : IAmazonQueueEventBus
{
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly IConfiguration _configuration;

    public AmazonQueueEventBus(
        AmazonSimpleNotificationServiceClient snsClient,
        IConfiguration configuration)
    {
        _snsClient = snsClient;
        _configuration = configuration;
    }

    public async Task Publish(IntegrationEvent @event, string topicName)
    {
        var topicArn = _configuration.GetValue<string>($"EventBus:Topics:{topicName}");

        var eventName = @event.GetType().Name;
        var body = JsonSerializer.Serialize(@event, @event.GetType());

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = body,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "event", new MessageAttributeValue() { StringValue = eventName, DataType = "String" } }
            }
        };

        await _snsClient.PublishAsync(request);
    }
}
