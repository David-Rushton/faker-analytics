using System.ComponentModel.DataAnnotations;

namespace Dr.FakerAnalytics.Api.Model;

public record PublicTrade(
    DateTimeOffset Timestamp,
    string TradeId,
    string Venue,
    decimal Price,
    decimal Quantity
);
