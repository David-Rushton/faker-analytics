using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddTransient<TradeGenerator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();


app.MapGet("/api/trades", (
    [FromQuery] DateTimeOffset from,
    [FromQuery] DateTimeOffset until,
    [FromQuery] int? instrumentId,
    [FromQuery] int? commingleId,
    [FromQuery] int sequenceId,
    [FromQuery] int sequenceItemId,
    [FromQuery] ContractType contractType,
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

    1, and only 1, instrumentId and commingleId must be supplied.

    from must be before until.
""");


app.Run();
