using Dr.TimeService.Services;

namespace Dr.TimeService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNowServices(this IServiceCollection services)
    {
        services
            .AddHostedService<ToolAdvertisingService>();

        services.AddHttpClient("tool-discovery-service", client =>
        {
            client.BaseAddress = new Uri("http://tool-discovery-service");
        });

        services.AddHealthChecks()
            .AddCheck("time-service-ready", () =>
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Time service is ready"));

        return services;
    }
}
