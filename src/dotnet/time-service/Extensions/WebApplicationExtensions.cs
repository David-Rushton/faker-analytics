using Dr.TimeService.Model;
using Microsoft.AspNetCore.Mvc;

namespace Dr.TimeService.Extensions;

public static class WebApplicationExtensions
{
    private const string RFC3339 = "o";
    public static WebApplication EnableEnvironmentSpecifics(this WebApplication app)
    {
        var logger = app.Logger;
        logger.LogInformation("Time Service starting up...");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            logger.LogInformation("OpenAPI documentation enabled for development");
        }

        return app;
    }

    public static WebApplication AddNowEndpoints(this WebApplication app)
    {
        app.AddNowEndpoint();

        return app;
    }

    private static WebApplication AddNowEndpoint(this WebApplication app)
    {
        app.MapGet("/api/now", (
            [FromServices] TimeProvider timeProvider,
            [FromServices] ILogger<Program> logger
        ) =>
        {
            logger.LogInformation("GET /api/now called");

            return Results.Ok(new NowResponse{ Now = timeProvider.GetUtcNow().ToString(RFC3339) });
        })
        .WithName("now")
        .WithDescription("Returns the current UTC date and time, in RFC3339 format.")
        .Produces<NowResponse>();

        return app;
    }
}
