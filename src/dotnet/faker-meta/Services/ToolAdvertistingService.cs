using Dr.ToolDiscoveryService.Abstractions;

namespace Dr.FakerMeta.Services;

public class ToolAdvertisingService(
    ILogger<ToolAdvertisingService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Tool[] tools = [GetInstrumentsTool(), GetDeliveriesTool(), GetInstrumentDeliveriesTool()];
        var httpClient = httpClientFactory.CreateClient("tool-discovery-service");

        logger.LogInformation("Starting tool advertising service.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var tool in tools)
                {
                    await httpClient.PutAsJsonAsync<Tool>($"/api/tools/{tool.Name}", tool, stoppingToken);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Tool advertising failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private string GetServiceBaseUrl()
    {
        // For external consumers (like CLI tools), use localhost
        // For internal Aspire services, they'll use service discovery
        var urls = configuration.GetValue<string>("ASPNETCORE_URLS") ?? "http://localhost:5222";

        // Extract the first HTTP URL for external access
        var firstUrl = urls.Split(';').FirstOrDefault(u => u.StartsWith("http://"));
        if (firstUrl != null)
        {
            return firstUrl;
        }

        // Fallback to localhost if we can't determine the URL
        return "http://localhost:5222";
    }

    private Tool GetInstrumentsTool()
    {
        var description = """
Returns an array containing all instruments.

Each returned instrument has an id, name and description.
""";
        var definition = new ToolDefinitionBuilder()
                .WithName("get_instruments")
                .WithDescription(description)
                .Build();

        return new Tool
        {
            ToolDefinition = definition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/meta/instruments")
            }
        };
    }

    private Tool GetDeliveriesTool()
    {
        var description = """
Returns an array containing all deliveries.

A delivery defines when a purchased instrument will be delivered.  Delivery make take place a fixed
point in time, or over some window (example delivering daily every working day of some month).

Each returned delivery has an id, name and description.
""";
        var definition = new ToolDefinitionBuilder()
                .WithName("get_deliveries")
                .WithDescription(description)
                .Build();

        return new Tool
        {
            ToolDefinition = definition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/meta/deliveries")
            }
        };
    }

    private Tool GetInstrumentDeliveriesTool()
    {
        var description = """
Returns an array detailing the relationship between instruments and deliveries.

Each row contains and instrumentId and an array of all the linked deliveryIds.

Every instrument is linked to at least 1 delivery.
""";
        var definition = new ToolDefinitionBuilder()
                .WithName("get_instrument_deliveries")
                .WithDescription(description)
                .Build();

        return new Tool
        {
            ToolDefinition = definition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/meta/instruments/deliveries")
            }
        };
    }
}
