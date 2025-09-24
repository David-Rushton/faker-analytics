using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddTransient<TradeGenerator>()
    .AddTransient<OhlcvGenerator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.MapGet("/api/trades", (
    [FromQuery] DateTimeOffset from,
    [FromQuery] DateTimeOffset until,
    [FromQuery] int? instrumentId,
    [FromQuery] int? commingleId,
    [FromQuery] int deliveryId,
    [FromServices] TradeGenerator tradeGenerator
) =>
{
    if (instrumentId is null && commingleId is null)
        return Results.BadRequest("You must supply either instrumentId or commingleId.");

    if (instrumentId is not null && commingleId is not null)
        return Results.BadRequest("instrumentId and commingleId are mutually exclusive, you cannot provide both.");

    if (!(from < until))
        return Results.BadRequest("from must be before until.");

    return Results.Ok(tradeGenerator.Generate(from, until));
})
.WithName("Trades")
.WithDescription("""
    Returns trades.

    1, and only 1, instrumentId or commingleId must be supplied.

    from must be before until.
""")
.Produces<IEnumerable<PublicTrade>>();

app.MapGet("/api/trades/ohlcv", (
    [FromQuery] DateTimeOffset from,
    [FromQuery] DateTimeOffset until,
    [FromQuery] int? instrumentId,
    [FromQuery] int? commingleId,
    [FromQuery] int deliveryId,
    [FromServices] OhlcvGenerator ohlcvGenerator
) =>
{
    if (instrumentId is null && commingleId is null)
        return Results.BadRequest("You must supply either instrumentId or commingleId.");

    if (instrumentId is not null && commingleId is not null)
        return Results.BadRequest("instrumentId and commingleId are mutually exclusive, you cannot provide both.");

    if (!(from < until))
        return Results.BadRequest("from must be before until.");

    return Results.Ok(ohlcvGenerator.Generate(from, until));
})
.WithName("Trades OHLCV")
.WithDescription("""
    Returns open high low close volume (OHLCV) candles.

    1, and only 1, instrumentId or commingleId must be supplied.

    from must be before until.
""")
.Produces<IEnumerable<OhlcvCandle>>();




app.Run();
