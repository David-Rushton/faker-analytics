namespace Dr.FakerMeta.Model;

public class Instrument
{
    public required int InstrumentId { get; init; }
    public required string InstrumentName { get; init; }
    public required string InstrumentDescription { get; init; }
}

public class Delivery
{
    public required int DeliveryId { get; init; }
    public required string DeliveryName { get; init; }
    public required string DeliveryDescription { get; init; }
}

public class InstrumentDeliveries
{
    public required int InstrumentId { get; init; }
    public required List<int>? DeliveryIds { get; init; }
}
