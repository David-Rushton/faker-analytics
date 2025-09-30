namespace Dr.FakerMeta.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakerMetaServices(this IServiceCollection services)
    {
        services
            .AddSingleton<InstrumentDeliveryRepository>()
            .AddHostedService<ToolAdvertisingService>();

        services.AddHttpClient("tool-discovery-service", client =>
        {
            client.BaseAddress = new Uri("http://tool-discovery-service");
        });

        services.AddHealthChecks()
            .AddCheck("faker-meta-ready", () =>
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Faker Meta is ready"));

        return services;
    }
}
