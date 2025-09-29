namespace Dr.FakerAnalytics.Api;

public class ToolAdvertisingService(
    ILogger<ToolAdvertisingService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Tool[] tools = [GetTradeTool(), GetOhlcvTool()];
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
        var urls = configuration.GetValue<string>("ASPNETCORE_URLS") ?? "http://localhost:5000";

        // Extract the first HTTP URL for external access
        var firstUrl = urls.Split(';').FirstOrDefault(u => u.StartsWith("http://"));
        if (firstUrl != null)
        {
            return firstUrl;
        }

        // Fallback to localhost if we can't determine the URL
        return "http://localhost:5000";
    }

    private Tool GetOhlcvTool()
    {
        var OhlcvDescription = """
Returns open high low close volume (OHLCV) candles.

1, and only 1, of instrumentId or commingleId must be supplied.

from must be before until.

Supported instrumentId, commingleId and deliveryId values are documented in the /api/metadata endpoints.
""";
        var ohlcvDefinition = new ToolDefinitionBuilder()
                .WithName("OHLCV")
                .WithDescription(OhlcvDescription)
                .WithParameters(p => p
                    .WithRequiredProperty("from", prop => prop
                        .WithType("string")
                        .WithDescription("Inclusive start date time.  Use RFC3339 format.  Must be before until."))
                    .WithRequiredProperty("until", prop => prop
                        .WithType("string")
                        .WithDescription("Exclusive end date time.  Use RFC3339 format.  Must be after from."))
                    .WithProperty("instrumentId", prop => prop
                        .WithType("number")
                        .WithDescription("The instrument to query.  Cannot be combined with commingleId."))
                    .WithProperty("commingleId", prop => prop
                        .WithType("number")
                        .WithDescription("The market to query.  Each commingled market contains 1 or more related instruments.  Cannot be combined with instrumentId."))
                    .WithProperty("deliveryId", prop => prop
                        .WithType("number")
                        .WithDescription("Describes how the instrument, or in the case of a commingled market instruments, will be delivered.")))
                .Build();

        return new Tool
        {
            ToolDefinition = ohlcvDefinition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/trades/ohlcv")
            }
        };
    }

    private Tool GetTradeTool()
    {
        var tradesDescription = """
Returns trades.

1, and only 1, of instrumentId or commingleId must be supplied.

from must be before until.

Supported instrumentId, commingleId and deliveryId values are documented in the /api/metadata endpoints.
""";

        var tradesDefinition =
            new ToolDefinitionBuilder()
                .WithName("Trades")
                .WithDescription(tradesDescription)
                .WithParameters(p => p
                    .WithRequiredProperty("from", prop => prop
                        .WithType("string")
                        .WithDescription("Inclusive start date time.  Use RFC3339 format.  Must be before until."))
                    .WithRequiredProperty("until", prop => prop
                        .WithType("string")
                        .WithDescription("Exclusive end date time.  Use RFC3339 format.  Must be after from."))
                    .WithProperty("instrumentId", prop => prop
                        .WithType("number")
                        .WithDescription("The instrument to query.  Cannot be combined with commingleId."))
                    .WithProperty("commingleId", prop => prop
                        .WithType("number")
                        .WithDescription("The market to query.  Each commingled market contains 1 or more related instruments.  Cannot be combined with instrumentId."))
                    .WithProperty("deliveryId", prop => prop
                        .WithType("number")
                        .WithDescription("Describes how the instrument, or in the case of a commingled market instruments, will be delivered.")))
                .Build();

        return new Tool
        {
            ToolDefinition = tradesDefinition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/trades")
            }
        };
    }
}
