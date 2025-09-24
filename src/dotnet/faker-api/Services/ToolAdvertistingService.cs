
using Dr.ToolDiscoveryService.Abstractions;

namespace Dr.FakerAnalytics.Api;

public class ToolAdvertisingService(
    ILogger<ToolAdvertisingService> logger,
    IHostLifetime hostLifetime,
    IWebHostEnvironment webEnvironment) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var tradeTool = GetTradeTool();
            var ohlcvTool = GetOhlcvTool();

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Starting tool discovery service.");
                throw new NotImplementedException();
            }
        }
        catch (Exception e)
        {
            logger.LogError("Tool discovery service failed: {err}", e);
            Environment.ExitCode = 1;
            await hostLifetime.StopAsync(stoppingToken);
        }
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
                .WithName("Trades")
                .WithDescription(OhlcvDescription)
                .WithParameters(p => p
                    .WithRequiredProperty("from", p => p
                        .WithType("string")
                        .WithDescription("Inclusive start date time.  Use RFC3339 format.  Must be before until.")))
                .WithParameters(p => p
                    .WithRequiredProperty("until", p => p
                        .WithType("string")
                        .WithDescription("Exclusive end date time.  Use RFC3339 format.  Must be after from.")))
                .WithParameters(p => p
                    .WithProperty("instrumentId", p => p
                        .WithType("number")
                        .WithDescription("The instrument to query.  Cannot be combined with commingleId.")))
                .WithParameters(p => p
                    .WithProperty("commingleId", p => p
                        .WithType("number")
                        .WithDescription("The market to query.  Each commingled market contains 1 or more related instruments.  Cannot be combined with instrumentId.")))
                .WithParameters(p => p
                    .WithProperty("deliveryId", p => p
                        .WithType("number")
                        .WithDescription("Describes how the instrument, or in the case of a commingled market instruments, will be delivered.")))
                .Build();

        return new Tool
        {
            ToolDefinition = ohlcvDefinition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{webEnvironment.ContentRootPath}/api/trades/ohlcv")
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
                    .WithRequiredProperty("from", p => p
                        .WithType("string")
                        .WithDescription("Inclusive start date time.  Use RFC3339 format.  Must be before until.")))
                .WithParameters(p => p
                    .WithRequiredProperty("until", p => p
                        .WithType("string")
                        .WithDescription("Exclusive end date time.  Use RFC3339 format.  Must be after from.")))
                .WithParameters(p => p
                    .WithProperty("instrumentId", p => p
                        .WithType("number")
                        .WithDescription("The instrument to query.  Cannot be combined with commingleId.")))
                .WithParameters(p => p
                    .WithProperty("commingleId", p => p
                        .WithType("number")
                        .WithDescription("The market to query.  Each commingled market contains 1 or more related instruments.  Cannot be combined with instrumentId.")))
                .WithParameters(p => p
                    .WithProperty("deliveryId", p => p
                        .WithType("number")
                        .WithDescription("Describes how the instrument, or in the case of a commingled market instruments, will be delivered.")))
                .Build();

        return new Tool
        {
            ToolDefinition = tradesDefinition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{webEnvironment.ContentRootPath}/api/trades")
            }
        };
    }
}
