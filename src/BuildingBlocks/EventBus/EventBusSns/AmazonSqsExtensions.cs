using System;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;

namespace EventBusSns
{
    public static class AmazonSqsExtensions
    {
        public static IServiceCollection AddAmazonSqsQueue(
            this IServiceCollection services,
            string queueUrl,
            Action<IAmazonQueueBuilder> queueBuilderAction)
        {
            if (queueBuilderAction == null)
            {
                throw new ArgumentNullException(nameof(queueBuilderAction));
            }

            var queueBuilder = new AmazonQueueBuilder(services);
            queueBuilderAction(queueBuilder);
            var queueMessageTypes = queueBuilder.Build();

            services.AddHostedService(sp =>
                new AmazonQueueReceiverHostedService(
                    queueUrl,
                    queueMessageTypes,
                    sp,
                    sp.GetRequiredService<AmazonSQSClient>()));

            return services;
        }
    }
}
