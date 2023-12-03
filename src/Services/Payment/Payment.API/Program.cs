using Amazon.SimpleNotificationService;
using Amazon.SQS;
using EventBusSns;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<PaymentSettings>(builder.Configuration);

builder.Services.AddTransient<OrderStatusChangedToStockConfirmedIntegrationEventHandler>();

builder.Services
    .AddSingleton(sp => new AmazonSQSClient())
    .AddSingleton(sp => new AmazonSimpleNotificationServiceClient())
    .AddSingleton<IAmazonQueueEventBus, AmazonQueueEventBus>()
    .AddAmazonSqsQueue(
        "https://sqs.us-east-1.amazonaws.com/705378975957/eshop_payment",
        opt =>
        {
            opt.AddEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
        });

builder.Services.AddHealthChecks()
    .AddSnsTopicsAndSubscriptions(options =>
        {
            options.AddTopicAndSubscriptions("DotnetEshop");
        })
    .AddSqs(options =>
        {
            options.AddQueue("eshop_payment");
        },
        name: "eventbus");

var app = builder.Build();

app.UseServiceDefaults();

await app.RunAsync();
