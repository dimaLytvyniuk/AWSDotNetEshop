using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EventBusSns;

public class AmazonQueueReceiverHostedService : BackgroundService
{
    private readonly string _queueUrl;
    private readonly IReadOnlyDictionary<string, Type> _queueMessageTypes;

    private readonly IServiceProvider _serviceProvider;
    private readonly AmazonSQSClient _sqsClient;

    public AmazonQueueReceiverHostedService(
        string queueUrl,
        IReadOnlyDictionary<string, Type> queueMessageTypes,
        IServiceProvider serviceProvider,
        AmazonSQSClient sqsClient)
    {
        _queueUrl = queueUrl;
        _queueMessageTypes = queueMessageTypes;
        _serviceProvider = serviceProvider;
        _sqsClient = sqsClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<AmazonQueueReceiverHostedService>>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var messageResponse = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 2
            });

            if (messageResponse.Messages.Count != 0)
            {
                foreach (var message in messageResponse.Messages)
                {
                    try
                    {
                        await HandleMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception during message handling");
                    }
                }
            }
        }
    }

    private async Task HandleMessageAsync(Message message)
    {
        var serializedMessageBody = JsonDocument.Parse(message.Body);
        var attributes = serializedMessageBody.RootElement.GetProperty("MessageAttributes");
        var attributesDictionary = attributes.Deserialize<Dictionary<string, Dictionary<string, string>>>();

        if (attributesDictionary.TryGetValue("event", out var eventNameDict))
        {
            var messageTypeName = eventNameDict["Value"];
            var messageType = _queueMessageTypes[messageTypeName];

            var genericType = typeof(AmazonQueueMessageHandler<>).MakeGenericType(messageType);
            var messageHandler = (IAmazonQueueMessageHandler)Activator.CreateInstance(genericType, _serviceProvider);

            var messageElement = serializedMessageBody.RootElement.GetProperty("Message");
            var messageJson = messageElement.GetString();

            await messageHandler.HandleMessageAsync(messageJson);
        }
        else
        {
            throw new ArgumentException();
        }

        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
    }
}
