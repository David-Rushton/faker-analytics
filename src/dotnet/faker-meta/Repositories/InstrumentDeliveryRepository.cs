using System.Collections.Concurrent;

namespace Dr.FakerMeta.Repositories;


public class InstrumentDeliveryRepository
{
    private readonly ILogger<InstrumentDeliveryRepository> _logger;
    private readonly TimeProvider _timeProvider;

    public InstrumentDeliveryRepository(
        ILogger<InstrumentDeliveryRepository> logger,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        GenerateData();
    }

    public ConcurrentBag<Instrument> Instruments { get;} = new();
    public ConcurrentBag<Delivery> Deliveries { get;} = new();
    public ConcurrentBag<InstrumentDeliveries> InstrumentsDeliveries { get;} = new();

    private void GenerateData()
    {
        _logger.LogInformation("Generating instrument and delivery meta data...");

        HashSet<int> periodInstruments = new();
        HashSet<int> spotInstruments = new();
        var instrumentId = 1;
        HashSet<int> periodDeliveries = new();
        HashSet<int> spotDeliveries = new();
        var deliveryId = 1;

        // Add period instruments.
        foreach (var country in new string[] { "UK", "German", "France", "Spain", "Dutch" })
        {
            foreach (var type in new string[] { "Baseload", "Peek", "Off Peak", "GAS" })
            {
                var subType = type == "GAS" ? "Gas" : "Electric";

                Instruments.Add(new Instrument
                {
                    InstrumentId = instrumentId,
                    InstrumentName = $"PERIOD-{country}-{type}".ToUpper().Replace(" ", "_"),
                    InstrumentDescription = $"{country} {type} {subType}"
                });

                periodInstruments.Add(instrumentId);
                instrumentId++;
            }
        }

        // Add spot instruments.
        foreach (var country in new string[] { "UK", "German", "France", "Spain", "Dutch" })
        {
            Instruments.Add(new Instrument
            {
                InstrumentId = instrumentId,
                InstrumentName = $"SPOT-{country}".ToUpper().Replace(" ", "_"),
                InstrumentDescription = $"{country} spot power"
            });

            spotInstruments.Add(instrumentId);
            instrumentId++;
        }


        // Add period deliveries.
        foreach (var year in Enumerable.Range(2010, 20))
        {
            // Add months.
            foreach (var month in Enumerable.Range(1, 12))
            {
                var period = new DateTime(year, month, 1);

                Deliveries.Add(new Delivery
                {
                    DeliveryId = deliveryId,
                    DeliveryName = $"MONTH-{period:yyyy-MMM}",
                    DeliveryDescription = $"Delivered weekdays during {month:MMMM yyyy}."
                });


                periodDeliveries.Add(deliveryId);
                deliveryId++;
            }

            // Add quarters.
            foreach (var quarter in Enumerable.Range(1, 4))
            {
                Deliveries.Add(new Delivery
                {
                    DeliveryId = deliveryId,
                    DeliveryName = $"QRT-{year}-Q{quarter}",
                    DeliveryDescription = $"Delivered during Q{quarter} {year}."
                });

                periodDeliveries.Add(deliveryId);
                deliveryId++;
            }
        }

        // Add spot deliveries.
        var spotDate = _timeProvider.GetUtcNow().AddDays(-30);
        var spotEnd = _timeProvider.GetUtcNow().AddDays(90);
        while (spotDate < spotEnd)
        {
            Deliveries.Add(new Delivery
            {
                DeliveryId = deliveryId,
                DeliveryName = $"SPOT-{spotDate:yyyy-MM-dd}",
                DeliveryDescription = $"Spot energy to be delivered during {spotDate:yyyy-MM-dd}."
            });

            spotDate = spotDate.AddDays(1);

            spotDeliveries.Add(deliveryId);
            deliveryId++;
        }

        // Build instrument delivery lookups.
        foreach (var instId in periodInstruments)
            InstrumentsDeliveries.Add(new InstrumentDeliveries
            {
                InstrumentId = instId,
                DeliveryIds = [.. periodDeliveries]
            });

        foreach (var instId in spotInstruments)
            InstrumentsDeliveries.Add(new InstrumentDeliveries
            {
                InstrumentId = instId,
                DeliveryIds = [.. spotDeliveries]
            });

        _logger.LogInformation("Instrument and delivery meta data generated.");
    }
}
