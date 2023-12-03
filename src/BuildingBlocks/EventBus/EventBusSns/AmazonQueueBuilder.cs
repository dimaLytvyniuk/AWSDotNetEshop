using System;
using System.Collections.Generic;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventBusSns;

public class AmazonQueueBuilder : IAmazonQueueBuilder
{
    private readonly IServiceCollection _services;

    private readonly Dictionary<string, Type> _queueMessageTypes = new Dictionary<string, Type>();

    public AmazonQueueBuilder(IServiceCollection services)
    {
        _services = services;
    }

    IAmazonQueueBuilder IAmazonQueueBuilder.AddEventHandler<TMessage, TMessageReceiver>()
    {
        _services.AddTransient<IIntegrationEventHandler<TMessage>, TMessageReceiver>();

        var messageType = typeof(TMessage);
        var typeName = typeof(TMessage).Name;
        _queueMessageTypes.Add(typeName, messageType);

        return this;
    }

    public Dictionary<string, Type> Build()
    {
        return _queueMessageTypes;
    }
}
