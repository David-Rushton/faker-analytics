namespace Dr.FakerAnalytics.Api.Model;

public record OhlcvCandle(
    DateTimeOffset Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
)
{
    public static OhlcvCandle Create(DateTimeOffset timeOffset) =>
        new(timeOffset, 0m, 0m, 0m, 0m, 0m);
}
