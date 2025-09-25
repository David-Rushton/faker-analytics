namespace Dr.FakerAnalytics.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakerAnalyticsServices(this IServiceCollection services)
    {
        services
            .AddTransient<TradeGenerator>()
            .AddTransient<OhlcvGenerator>()
            .AddHostedService<ToolAdvertisingService>();

        services.AddHttpClient("tool-discovery-service", client =>
        {
            client.BaseAddress = new Uri("http://tool-discovery-service");
        });

        services.AddHealthChecks()
            .AddCheck("faker-api-ready", () =>
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Faker API is ready"));

        return services;
    }
}
