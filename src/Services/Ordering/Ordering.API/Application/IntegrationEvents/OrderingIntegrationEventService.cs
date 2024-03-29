﻿using EventBusSns;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents;

public class OrderingIntegrationEventService : IOrderingIntegrationEventService
{
    private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
    private readonly IAmazonQueueEventBus _eventBus;
    private readonly OrderingContext _orderingContext;
    private readonly IIntegrationEventLogService _eventLogService;
    private readonly ILogger<OrderingIntegrationEventService> _logger;

    public OrderingIntegrationEventService(IAmazonQueueEventBus eventBus,
        OrderingContext orderingContext,
        IntegrationEventLogContext eventLogContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory,
        ILogger<OrderingIntegrationEventService> logger)
    {
        _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
        _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventLogService = _integrationEventLogServiceFactory(_orderingContext.Database.GetDbConnection());
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

        foreach (var logEvt in pendingLogEvents)
        {
            _logger.LogInformation("Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", logEvt.EventId, logEvt.IntegrationEvent);

            try
            {
                await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                await _eventBus.Publish(logEvt.IntegrationEvent, "eshop");
                await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing integration event: {IntegrationEventId}", logEvt.EventId);

                await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    public async Task AddAndSaveEventAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);

        await _eventLogService.SaveEventAsync(evt, _orderingContext.GetCurrentTransaction());
    }
}
