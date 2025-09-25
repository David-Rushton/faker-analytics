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

        return services;
    }
}
