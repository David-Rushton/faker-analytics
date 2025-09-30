using Microsoft.AspNetCore.Mvc;

namespace Dr.FakerAnalytics.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication EnableEnvironmentSpecifics(this WebApplication app)
    {
        var logger = app.Logger;
        logger.LogInformation("Faker Analytics API starting up...");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            logger.LogInformation("OpenAPI documentation enabled for development");
        }

        return app;
    }

    public static WebApplication AddFakerAnalyticsEndpoints(this WebApplication app)
    {
        app.AddTradesEndpoint();
        app.AddOhlcvEndpoint();

        return app;
    }

    private static WebApplication AddTradesEndpoint(this WebApplication app)
    {
        app.MapGet("/api/trades", (
            [FromQuery] DateTimeOffset from,
            [FromQuery] DateTimeOffset until,
            [FromQuery] int? instrumentId,
            [FromQuery] int? commingleId,
            [FromQuery] int deliveryId,
            [FromServices] TradeGenerator tradeGenerator,
            [FromServices] ILogger<Program> logger
        ) =>
        {
            logger.LogInformation("Processing trades request for {From} to {Until}, InstrumentId: {InstrumentId}, CommingleId: {CommingleId}, DeliveryId: {DeliveryId}",
                from, until, instrumentId, commingleId, deliveryId);

            if (instrumentId is null && commingleId is null)
            {
                logger.LogWarning("Bad request: Neither instrumentId nor commingleId provided");
                return Results.BadRequest("You must supply either instrumentId or commingleId.");
            }

            if (instrumentId is not null && commingleId is not null)
            {
                logger.LogWarning("Bad request: Both instrumentId and commingleId provided");
                return Results.BadRequest("instrumentId and commingleId are mutually exclusive, you cannot provide both.");
            }

            if (!(from < until))
            {
                logger.LogWarning("Bad request: Invalid date range - from: {From}, until: {Until}", from, until);
                return Results.BadRequest("from must be before until.");
            }

            var trades = tradeGenerator.Generate(from, until);
            logger.LogInformation("Generated {TradeCount} trades for request", trades.Count());

            return Results.Ok(trades);
        })
        .WithName("Trades")
        .WithDescription("""
Returns trades.

1, and only 1, instrumentId or commingleId must be supplied.

from must be before until.
""")
        .Produces<IEnumerable<PublicTrade>>();

        return app;
    }

    private static WebApplication AddOhlcvEndpoint(this WebApplication app)
    {
        app.MapGet("/api/trades/ohlcv", (
            [FromQuery] DateTimeOffset from,
            [FromQuery] DateTimeOffset until,
            [FromQuery] int? instrumentId,
            [FromQuery] int? commingleId,
            [FromQuery] int deliveryId,
            [FromServices] OhlcvGenerator ohlcvGenerator,
            [FromServices] ILogger<Program> logger
        ) =>
        {
            logger.LogInformation("Processing OHLCV request for {From} to {Until}, InstrumentId: {InstrumentId}, CommingleId: {CommingleId}, DeliveryId: {DeliveryId}",
                from, until, instrumentId, commingleId, deliveryId);

            if (instrumentId is null && commingleId is null)
            {
                logger.LogWarning("Bad request: Neither instrumentId nor commingleId provided");
                return Results.BadRequest("You must supply either instrumentId or commingleId.");
            }

            if (instrumentId is not null && commingleId is not null)
            {
                logger.LogWarning("Bad request: Both instrumentId and commingleId provided");
                return Results.BadRequest("instrumentId and commingleId are mutually exclusive, you cannot provide both.");
            }

            if (!(from < until))
            {
                logger.LogWarning("Bad request: Invalid date range - from: {From}, until: {Until}", from, until);
                return Results.BadRequest("from must be before until.");
            }

            var candles = ohlcvGenerator.Generate(from, until);
            logger.LogInformation("Generated {CandleCount} OHLCV candles for request", candles.Count());

            return Results.Ok(candles);
        })
        .WithName("Trades OHLCV")
        .WithDescription("""
Returns open high low close volume (OHLCV) candles.

1, and only 1, instrumentId or commingleId must be supplied.

from must be before until.
""")
        .Produces<IEnumerable<OhlcvCandle>>();

        return app;
    }
}
