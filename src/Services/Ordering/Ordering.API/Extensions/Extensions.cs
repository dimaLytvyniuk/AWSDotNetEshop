using Microsoft.EntityFrameworkCore.Infrastructure;
using MySql.EntityFrameworkCore.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder
            .AddMySql(
                configuration.GetRequiredConnectionString("OrderingDB"),
                name: "OrderingDB-check",
                tags: new string[] { "ready" });

        hcBuilder
            .AddSnsTopicsAndSubscriptions(options =>
                {
                    options.AddTopicAndSubscriptions("DotnetEshop");
                })
            .AddSqs(options =>
                {
                    options.AddQueue("eshop_ordering");
                },
                name: "eventbus");

        return services;
    }

    public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        static void ConfigureMySqlOptions(MySQLDbContextOptionsBuilder sqlOptions)
        {
            sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        };

        services.AddDbContext<OrderingContext>(options =>
        {
            options.UseMySQL(configuration.GetRequiredConnectionString("OrderingDB"), ConfigureMySqlOptions);
        });

        services.AddDbContext<IntegrationEventLogContext>(options =>
        {
            options.UseMySQL(configuration.GetRequiredConnectionString("OrderingDB"), ConfigureMySqlOptions);
        });

        return services;
    }

    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
            sp => (DbConnection c) => new IntegrationEventLogService(c));

        services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();

        return services;
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OrderingSettings>(configuration);
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
            };
        });

        return services;
    }
}
