using System.Threading;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class DynamoHC : IHealthCheck
{
    IServiceProvider _sp;

    public DynamoHC(IServiceProvider sp)
    {
        _sp = sp;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
       var database = _sp.GetRequiredService<IAmazonDynamoDB>();
       return database.ListTablesAsync(new ListTablesRequest
        {
            Limit = 100,
            ExclusiveStartTableName = null
        }).WaitAsync(TimeSpan.FromSeconds(5))
        .ContinueWith((task1)=> {
            if (task1.IsFaulted) return HealthCheckResult.Unhealthy();
            else return task1.Result.HttpStatusCode == HttpStatusCode.OK ? HealthCheckResult.Healthy("Ok") : HealthCheckResult.Unhealthy();
        });
    }
}

public static class Extensions
{
    public static IServiceCollection AddDynamo(this IServiceCollection services, IConfiguration configuration)
    {
        var dynamoDbConfig = configuration.GetSection("DynamoDb");
        var runLocalDynamoDb = dynamoDbConfig.GetValue<bool>("LocalMode");

        return services.AddSingleton<IAmazonDynamoDB>(sp =>
        {
            if (runLocalDynamoDb)
            {
                var accessKey = configuration.GetValue<string>("AWS_ACCESS_KEY_ID");
                var awsSecret = configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY");

                return new AmazonDynamoDBClient(accessKey, awsSecret);
            }

            return new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);
        });
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();
        hcBuilder
            .AddDynamo(services,
                name: "BasketDB-check",
                tags: new string[] { "ready" });
        return services;
    }

    public static void AddDynamo(this IHealthChecksBuilder healthCheck, IServiceCollection services, string name, string[] tags)
    {
        healthCheck.Add(new HealthCheckRegistration(
            name,
            sp => new DynamoHC(sp),
            HealthStatus.Unhealthy,
            tags,
            TimeSpan.FromSeconds(5)));
    }

}
