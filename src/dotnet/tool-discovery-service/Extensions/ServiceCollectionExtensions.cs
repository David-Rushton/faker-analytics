namespace Dr.ToolDiscoveryService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakerAnalyticsServices(this IServiceCollection services)
    {
        services
            .AddOptions<ToolsServiceOptions>()
            .BindConfiguration("ToolDiscovery")
            .ValidateOnStart();

        services
            .AddTransient<ToolsService>()
            .AddHostedService<ToolExpiryService>();

        // Add health checks for tool-discovery-service
        services.AddHealthChecks()
            .AddCheck("tool-discovery-ready", () =>
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Tool Discovery Service is ready"));

        return services;
    }
}
