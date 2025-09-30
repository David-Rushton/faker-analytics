using Dr.ToolDiscoveryService.Abstractions;

namespace Dr.TimeService.Services;

public class ToolAdvertisingService(
    ILogger<ToolAdvertisingService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Tool[] tools = [GetNowTool()];
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
        var urls = configuration.GetValue<string>("ASPNETCORE_URLS") ?? "http://localhost:5050";

        // Extract the first HTTP URL for external access
        var firstUrl = urls.Split(';').FirstOrDefault(u => u.StartsWith("http://"));
        if (firstUrl != null)
        {
            return firstUrl;
        }

        // Fallback to localhost if we can't determine the URL
        return "http://localhost:5050";
    }

    private Tool GetNowTool()
    {
        var definition = new ToolDefinitionBuilder()
                .WithName("get_utc_now")
                .WithDescription("Returns the current UTC date and time, in RFC3339 format.")
                .Build();

        return new Tool
        {
            ToolDefinition = definition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/now")
            }
        };
    }
}
