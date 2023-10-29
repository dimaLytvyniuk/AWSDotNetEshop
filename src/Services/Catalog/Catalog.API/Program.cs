using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddControllers();

// Application specific services
builder.Services.AddHealthChecks(builder.Configuration);
builder.Services.AddDbContexts(builder.Configuration);
builder.Services.AddApplicationOptions(builder.Configuration);
builder.Services.AddIntegrationServices();

#if DEBUG 
builder.Services.AddTransient<OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
builder.Services.AddTransient<OrderStatusChangedToPaidIntegrationEventHandler>();
#endif

var app = builder.Build();

app.UseServiceDefaults();

app.MapPicApi();
app.MapControllers();
app.MapGrpcService<CatalogService>();

//TODO uncomment
//var eventBus = app.Services.GetRequiredService<IEventBus>();
//eventBus.Subscribe<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
//eventBus.Subscribe<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();


// REVIEW: This is done fore development east but shouldn't be here in production
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    var settings = app.Services.GetService<IOptions<CatalogSettings>>();
    var logger = app.Services.GetService<ILogger<CatalogContextSeed>>();

    var healthCheck = app.Services.GetService<HealthCheckService>();
    var healthReport = await healthCheck.CheckHealthAsync();

    var dbStatus = healthReport.Entries.First(e => e.Key == "CatalogDB-check");

    if (dbStatus.Value.Status == HealthStatus.Healthy)
    {
        await context.Database.MigrateAsync();

        await new CatalogContextSeed().SeedAsync(context, app.Environment, settings, logger);
        var integEventContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
        await integEventContext.Database.MigrateAsync();
    }
}

await app.RunAsync();
