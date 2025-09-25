using Microsoft.AspNetCore.Mvc;

namespace Dr.ToolDiscoveryService.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication EnableEnvironmentSpecifics(this WebApplication app)
    {
        var logger = app.Logger;
        logger.LogInformation("Tool Discovery Service starting up...");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            logger.LogInformation("OpenAPI documentation enabled for development");
        }

        return app;
    }

    public static WebApplication AddDiscoveryEndpoints(this WebApplication app)
    {
        app.AddPutToolEndpoint();
        app.AddGetToolsEndpoint();
        app.AddGetToolEndpoint();
        return app;
    }

    private static WebApplication AddPutToolEndpoint(this WebApplication app)
    {
        app.MapPut("/api/tools/{toolName}", (
            string toolName,
            [FromBody] Tool tool,
            [FromServices] ILogger<Program> logger,
            [FromServices] ToolsService toolsService
        ) =>
        {
            logger.LogInformation("PUT /api/tools/{toolName} request received.", tool.Name);

            if (toolName != tool.Name)
            {
                logger.LogInformation("Cannot put tool.  Tool name and definition do not match.  Expected {toolName} vs actual {actualToolName}", toolName, tool.Name);
                return Results.BadRequest("Tool name and definition do not match.");
            }

            toolsService.AddOrUpdate(tool);

            return Results.Ok();
        })
        .WithName("Tools")
        .WithDescription("""
Adds or updates a tool within the discovery service.

All tools should periodically call this endpoint to prevent the tool from being marketed as stale
and removed from the service.
""");

        return app;
    }

    private static WebApplication AddGetToolsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/tools", (
            string toolName,
            [FromServices] ILogger<Program> logger,
            [FromServices] ToolsService toolsService
        ) =>
        {
            logger.LogInformation("GET /api/tools request received.");

            Results.Ok(toolsService.List());
        })
        .WithName("Tools")
        .WithDescription("Lists all tools registered with the discovery service.");

        return app;
    }

    private static WebApplication AddGetToolEndpoint(this WebApplication app)
    {
        app.MapGet("/api/tools/{toolName}", (
            string toolName,
            [FromServices] ILogger<Program> logger,
            [FromServices] ToolsService toolsService
        ) =>
        {
            logger.LogInformation("GET /api/tools/{toolName} request received.", toolName);

            return toolsService.TryGet(toolName, out var tool)
                ? Results.Ok(tool)
                : Results.NotFound();
        })
        .WithName("Tools")
        .WithDescription("Details a tool registered with the discovery service.");

        return app;
    }
}
