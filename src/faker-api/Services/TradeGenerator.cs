using System.Data;

namespace Dr.FakerAnalytics.Api.Services;

public class TradeGenerator
{
    static string[] Venues = ["AXE", "MAJOR", "FAST-SPOT", "PREM-TRADES"];

    const int MinTradesPerPeriod = 5;
    const int MaxTradesPerPeriod = 50;
    const int MinPrice = 10_00;
    const int MaxPrice = 100_00;
    const int MinQuantity = 1_00;
    const int MaxQuantity = 50_00;

    private readonly Random _random = new();

    public IEnumerable<PublicTrade> Generate(DateTimeOffset from, DateTimeOffset until)
    {
        DateTimeOffset currentTimestamp = from;
        decimal currentPrice = _random.NextInt64(MinPrice, MaxPrice) / 100.0m;
        decimal currentQuantity = _random.NextInt64(MinQuantity, MaxQuantity) / 100.00m;

        while (currentTimestamp < until)
        {
            var tradesRequired = _random.NextInt64(MinTradesPerPeriod, MaxTradesPerPeriod);
            while (tradesRequired > 0)
            {
                var venue = getVenue();

                yield return new(
                    currentTimestamp,
                    getTradeId(currentTimestamp, venue),
                    venue,
                    currentPrice,
                    currentQuantity
                );

                tradesRequired--;
            }

            currentTimestamp = currentTimestamp.AddHours(1);
        }

        string getVenue() =>
            _random.GetItems(Venues, 1)[0];

        string getTradeId(DateTimeOffset timestamp, string venue) =>
            $"{timestamp.Ticks}.{venue}.{_random.NextInt64(1_000_000, 2_000_000)}";
    }
}
