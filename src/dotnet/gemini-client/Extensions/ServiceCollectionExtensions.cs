using Microsoft.Extensions.DependencyInjection;

namespace Dr.GeminiClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeminiClient(this IServiceCollection services)
    {
        services.AddTransient<GeminiClient>();
        services.AddOptions<GeminiClientOptions>()
            .BindConfiguration("GeminiClient")
            .ValidateOnStart();

        return services;
    }
}
