namespace Dr.ChartingService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChartingServices(this IServiceCollection services)
    {
        services
            .AddSingleton<WebpageRepository>()
            .AddHostedService<ToolAdvertisingService>();

        services.AddHttpClient("tool-discovery-service", client =>
        {
            client.BaseAddress = new Uri("http://tool-discovery-service");
        });

        services.AddHealthChecks()
            .AddCheck("charting-servie-ready", () =>
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Faker Meta is ready"));

        return services;
    }
}
