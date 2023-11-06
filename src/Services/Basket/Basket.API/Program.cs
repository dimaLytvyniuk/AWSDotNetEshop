﻿var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseServiceDefaults();

app.MapGrpcService<BasketService>();
app.MapControllers();

//var eventBus = app.Services.GetRequiredService<IEventBus>();

//eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();
//eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

await app.RunAsync();
