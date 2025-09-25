namespace Dr.FakerAnalytics.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakerAnalyticsServices(this IServiceCollection services)
    {
        services
            .AddTransient<TradeGenerator>()
            .AddTransient<OhlcvGenerator>();

        return services;
    }
}
