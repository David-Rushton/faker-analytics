using Microsoft.AspNetCore.Mvc;

namespace Dr.FakerMeta.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication EnableEnvironmentSpecifics(this WebApplication app)
    {
        var logger = app.Logger;
        logger.LogInformation("Faker Meta API starting up...");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            logger.LogInformation("OpenAPI documentation enabled for development");
        }

        return app;
    }

    public static WebApplication AddFakerMetaEndpoints(this WebApplication app)
    {
        app.AddInstrumentsEndpoint();
        app.AddDeliveriesEndpoint();
        app.AddInstrumentDeliveriesEndpoint();

        return app;
    }

    private static WebApplication AddInstrumentsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/meta/instruments", (
            [FromServices] ILogger<Program> logger,
            [FromServices] InstrumentDeliveryRepository repository
        ) =>
        {
            logger.LogInformation("GET api/meta/instruments");

            return Results.Ok(repository.Instruments);
        })
        .WithName("get_instruments")
        .WithDescription("""
Returns an array containing all instruments.

Each returned instrument has an id, name and description.
""")
        .Produces<IEnumerable<Instrument[]>>();

        return app;
    }

    private static WebApplication AddDeliveriesEndpoint(this WebApplication app)
    {
        app.MapGet("/api/meta/deliveries", (
            [FromServices] ILogger<Program> logger,
            [FromServices] InstrumentDeliveryRepository repository
        ) =>
        {
            logger.LogInformation("GET api/meta/deliveries");

            return Results.Ok(repository.Deliveries);
        })
        .WithName("get_deliveries")
        .WithDescription("""
Returns an array containing all deliveries.

A delivery defines when a purchased instrument will be delivered.  Delivery make take place a fixed
point in time, or over some window (example delivering daily every working day of some month).

Each returned delivery has an id, name and description.
""")
        .Produces<IEnumerable<Delivery[]>>();

        return app;
    }

    private static WebApplication AddInstrumentDeliveriesEndpoint(this WebApplication app)
    {
        app.MapGet("/api/meta/instruments/deliveries", (
            [FromServices] ILogger<Program> logger,
            [FromServices] InstrumentDeliveryRepository repository
        ) =>
        {
            logger.LogInformation("GET api/meta/instruments/deliveries");

            return Results.Ok(repository.InstrumentsDeliveries);
        })
        .WithName("get_instrument_deliveries")
        .WithDescription("""
Returns an array detailing the relationship between instruments and deliveries.

Each row contains and instrumentId and an array of all the linked deliveryIds.

Every instrument is linked to at least 1 delivery.
""")
        .Produces<IEnumerable<InstrumentDeliveries[]>>();

        return app;
    }
}
