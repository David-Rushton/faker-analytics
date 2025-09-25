namespace Dr.ToolDiscoveryService.Services;

public class ToolExpiryService(
    ILogger<ToolExpiryService> logger,
    ToolsService toolsService) : BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting tool expiry service.");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Checking for expired tools.");
            toolsService.RemoveExpiredTools();

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
