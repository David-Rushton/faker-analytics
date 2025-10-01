using Dr.ToolDiscoveryService.Abstractions;

namespace Dr.ChartingService.Services;

public class ToolAdvertisingService(
    ILogger<ToolAdvertisingService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Tool[] tools = [PostWebpageTool(), GetWebpageTool()];
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
        var urls = configuration.GetValue<string>("ASPNETCORE_URLS") ?? "http://localhost:5254";

        // Extract the first HTTP URL for external access
        var firstUrl = urls.Split(';').FirstOrDefault(u => u.StartsWith("http://"));
        if (firstUrl != null)
        {
            return firstUrl;
        }

        // Fallback to localhost if we can't determine the URL
        return "http://localhost:5254";
    }

    private Tool PostWebpageTool()
    {
        var description = """
Generates a webpage, using the provided natural language prompt.

Webpages are primarily designed to chart datasets.

Requests for a webpage should include the data in the prompt.
""";
        var definition = new ToolDefinitionBuilder()
                .WithName("create_webpage")
                .WithDescription(description)
                .WithParameters(p =>
                    p.WithRequiredProperty("prompt", p => p
                        .WithType("string")
                        .WithDescription("Nature language prompt from which the webpage is constructed.")
                        .Build()))
                .Build();

        return new Tool
        {
            ToolDefinition = definition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Post,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/webpages"),
                RequiresGenAiKey = true
            }
        };
    }

    private Tool GetWebpageTool()
    {
        var description = """
Returns a webpage.

Webpages generated via POST /api/webpage are persisted and can be recalled later.
""";
        var definition = new ToolDefinitionBuilder()
                .WithName("get_webpage")
                .WithDescription(description)
                .WithParameters(p => p
                    .WithRequiredProperty("webpageId", p => p
                    .WithType("string")
                    .WithDescription("Webpage unique id")
                    .Build()))
                .Build();

        return new Tool
        {
            ToolDefinition = definition,
            ToolRoute = new()
            {
                HttpRequestMethod = HttpRequestMethod.Get,
                Uri = new Uri($"{GetServiceBaseUrl()}/api/webpages/{{webpageId}}")
            }
        };
    }
}
