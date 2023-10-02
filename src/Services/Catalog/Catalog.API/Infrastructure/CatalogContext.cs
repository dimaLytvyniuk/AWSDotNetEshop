namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }

    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    public DbSet<CatalogType> CatalogTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
    }
}


public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    public CatalogContext CreateDbContext(string[] args)
    {
        // TODO: need to use this approach everywhere in other projects with db contexts.
        var environment = Environment.GetEnvironmentVariable("Environment");
        environment = environment ?? "Development";

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{environment}.json")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("CatalogDB");

        var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>().UseMySQL(connectionString);

        return new CatalogContext(optionsBuilder.Options);
    }
}
