using Amazon.SimpleNotificationService;
using Amazon.SQS;
using EventBusSns;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddDynamo(builder.Configuration);
builder.Services.AddHealthChecks(builder.Configuration);

builder.Services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();

builder.Services.AddTransient<IBasketRepository, DynamoBasketRepository>();
builder.Services.AddTransient<IIdentityService, IdentityService>();

builder.Services
    .AddSingleton(sp => new AmazonSQSClient())
    .AddSingleton(sp => new AmazonSimpleNotificationServiceClient())
    .AddSingleton<IAmazonQueueEventBus, AmazonQueueEventBus>()
    .AddAmazonSqsQueue(
        "https://sqs.us-east-1.amazonaws.com/705378975957/eshop_basket",
        opt =>
        {
            opt.AddEventHandler<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();
            opt.AddEventHandler<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
        });

var app = builder.Build();

app.UseServiceDefaults();

app.MapGrpcService<BasketService>();
app.MapControllers();

await app.RunAsync();
