using System.ComponentModel.DataAnnotations;

namespace Dr.FakerAnalytics.Api.Model;

public record PublicTrade(
    DateTimeOffset Timestamp,
    string TradeId,
    string Venue,
    string Symbol,
    decimal Price,
    decimal Quantity
);
