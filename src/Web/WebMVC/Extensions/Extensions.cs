// Fix samesite issue when running eShop from docker-compose locally as by default http protocol is being used
// Refer to https://github.com/dotnet-architecture/eShopOnContainers/issues/1391

using Polly;
using Polly.Extensions.Http;
using Yarp.ReverseProxy.Forwarder;

internal static class Extensions
{
    private const int RetryCounts = 4;

    public static void AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddUrlGroup(_ => new Uri(configuration["IdentityUrlHC"]), name: "identityapi-check", tags: new string[] { "identityapi" });
    }

    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration);
    }

    // Adds all Http client services
    public static void AddHttpClientServices(this IServiceCollection services)
    {
        // Register delegating handlers
        services.AddTransient<HttpClientAuthorizationDelegatingHandler>()
                .AddTransient<HttpClientRequestIdDelegatingHandler>();

        // Add http client services
        services.AddHttpClient<IBasketService, BasketService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) //Sample. Default lifetime is 2 minutes
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

        services.AddHttpClient<ICatalogService, CatalogService>()
                .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IOrderingService, OrderingService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddHttpMessageHandler<HttpClientRequestIdDelegatingHandler>();

        // Add custom application services
        services.AddTransient<IIdentityParser<ApplicationUser>, IdentityParser>();
    }

    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        var identityUrl = configuration.GetRequiredValue("IdentityUrl");
        var callBackUrl = configuration.GetRequiredValue("CallBackUrl");
        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        // Add Authentication services
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
        .AddOpenIdConnect(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = identityUrl;
            options.SignedOutRedirectUri = callBackUrl;
            options.ClientId = "mvc";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("orders");
            options.Scope.Add("basket");
            options.Scope.Add("webshoppingagg");
            options.Scope.Add("orders.signalrhub");
        });
    }

    public static IEndpointConventionBuilder MapForwardSignalR(this WebApplication app)
    {
        // Forward the SignalR traffic to the bff
        var destination = app.Configuration.GetRequiredValue("PurchaseUrl");
        var authTransformer = new BffAuthTransformer();
        var requestConfig = new ForwarderRequestConfig();

        return app.MapForwarder("/hub/notificationhub/{**any}", destination, requestConfig, authTransformer);
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(RetryCounts, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private sealed class BffAuthTransformer : HttpTransformer
    {
        public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
        {
            // Set the access token as a bearer token for the outgoing request
            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (accessToken is not null)
            {
                proxyRequest.Headers.Authorization = new("Bearer", accessToken);
            }

            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        }
    }
}
