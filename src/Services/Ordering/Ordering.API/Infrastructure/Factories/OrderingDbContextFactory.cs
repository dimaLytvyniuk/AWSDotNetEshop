namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Factories
{
    public class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingContext>
    {
        public OrderingContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("Environment");
            environment = environment ?? "Development";

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environment}.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("OrderingDB");

            var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>()
                .UseMySQL(connectionString, mySqlOptionsAction: o => o.MigrationsAssembly("Ordering.API"));

            return new OrderingContext(optionsBuilder.Options);
        }
    }
}
