using System.Data;
using System.Diagnostics;

namespace Dr.FakerAnalytics.Api.Services;

public class OhlcvGenerator(TradeGenerator tradeGenerator)
{
    public IEnumerable<OhlcvCandle> Generate(DateTimeOffset from, DateTimeOffset until)
    {
        var trades = tradeGenerator.Generate(from, until);
        var results = new Dictionary<DateTimeOffset, OhlcvCandle>();

        foreach (var trade in trades)
        {
            if (!results.Keys.Contains(trade.Timestamp))
            {
                results[trade.Timestamp] = OhlcvCandle.Create(trade.Timestamp) with
                {
                    Open = trade.Price,
                    High = decimal.MinValue,
                    Low = decimal.MaxValue
                };
            }

            var current = results[trade.Timestamp];

            if (trade.Price > current.High)
                current = current with { High = trade.Price };

            if (trade.Price < current.Low)
                current = current with { Low = trade.Price };

            current = current with
            {
                Close = trade.Price,
                Volume = current.Volume + trade.Quantity
            };

            results[trade.Timestamp] = current;
        }

        return results
            .Select(kvp => kvp.Value)
            .OrderBy(ohlc => ohlc.Timestamp);
    }
}
