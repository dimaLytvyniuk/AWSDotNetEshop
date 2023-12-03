using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EventBusSns;
using Microsoft.Extensions.Options;
using Ordering.BackgroundTasks.Events;

namespace Ordering.BackgroundTasks.Services
{
    public class GracePeriodManagerService : BackgroundService
    {
        private readonly ILogger<GracePeriodManagerService> _logger;
        private readonly BackgroundTaskSettings _settings;
        private readonly IAmazonQueueEventBus _eventBus;

        public GracePeriodManagerService(IOptions<BackgroundTaskSettings> settings, IAmazonQueueEventBus eventBus, ILogger<GracePeriodManagerService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("GracePeriodManagerService is starting.");

            stoppingToken.Register(() => _logger.LogDebug("#1 GracePeriodManagerService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("GracePeriodManagerService background task is doing background work.");

                await CheckConfirmedGracePeriodOrders();
                await Task.Delay(_settings.CheckUpdateTime, stoppingToken);
            }

            _logger.LogDebug("GracePeriodManagerService background task is stopping.");
        }

        private async Task CheckConfirmedGracePeriodOrders()
        {
            _logger.LogDebug("Checking confirmed grace period orders");

            var orderIds = GetConfirmedGracePeriodOrders();

            foreach (var orderId in orderIds)
            {
                var confirmGracePeriodEvent = new GracePeriodConfirmedIntegrationEvent(orderId);

                _logger.LogInformation("Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", confirmGracePeriodEvent.Id, confirmGracePeriodEvent);

                await _eventBus.Publish(confirmGracePeriodEvent, "eshop");
            }
        }

        private IEnumerable<int> GetConfirmedGracePeriodOrders()
        {
            IEnumerable<int> orderIds = new List<int>();

            using var conn = new SqlConnection(_settings.ConnectionString);
            try
            {
                conn.Open();
                orderIds = conn.Query<int>(
                    @"SELECT Id FROM [ordering].[orders] 
                        WHERE DATEDIFF(minute, [OrderDate], GETDATE()) >= @GracePeriodTime
                        AND [OrderStatusId] = 1",
                    new { _settings.GracePeriodTime });
            }
            catch (SqlException exception)
            {
                _logger.LogCritical(exception, "Fatal error establishing database connection");
            }


            return orderIds;
        }
    }
}
