using System.Text.Json.Nodes;

var jsonContent = """
[
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.FAST-SPOT.1762230",
        "venue": "FAST-SPOT",
        "price": -307.25,
        "quantity": -1401.84
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.FAST-SPOT.1732563",
        "venue": "FAST-SPOT",
        "price": -302.27,
        "quantity": -1397.78
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.MAJOR.1858357",
        "venue": "MAJOR",
        "price": -301.90,
        "quantity": -1389.25
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.AXE.1077095",
        "venue": "AXE",
        "price": -304.69,
        "quantity": -1395.42
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.AXE.1202435",
        "venue": "AXE",
        "price": -313.01,
        "quantity": -1394.80
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.FAST-SPOT.1444007",
        "venue": "FAST-SPOT",
        "price": -310.43,
        "quantity": -1399.02
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.MAJOR.1946045",
        "venue": "MAJOR",
        "price": -318.76,
        "quantity": -1390.67
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.AXE.1140201",
        "venue": "AXE",
        "price": -326.44,
        "quantity": -1397.16
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.MAJOR.1697980",
        "venue": "MAJOR",
        "price": -316.65,
        "quantity": -1396.72
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.AXE.1189259",
        "venue": "AXE",
        "price": -313.93,
        "quantity": -1398.51
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.AXE.1834429",
        "venue": "AXE",
        "price": -320.93,
        "quantity": -1405.85
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.FAST-SPOT.1037722",
        "venue": "FAST-SPOT",
        "price": -330.60,
        "quantity": -1399.80
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.FAST-SPOT.1424300",
        "venue": "FAST-SPOT",
        "price": -321.03,
        "quantity": -1409.16
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.MAJOR.1802814",
        "venue": "MAJOR",
        "price": -326.18,
        "quantity": -1412.61
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.PREM-TRADES.1681961",
        "venue": "PREM-TRADES",
        "price": -327.35,
        "quantity": -1420.48
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.PREM-TRADES.1219165",
        "venue": "PREM-TRADES",
        "price": -332.99,
        "quantity": -1418.65
    },
    {
        "timestamp": "2024-11-30T23:00:00+00:00",
        "tradeId": "638686044000000000.MAJOR.1128520",
        "venue": "MAJOR",
        "price": -342.67,
        "quantity": -1411.92
    }
]
""";

var jsonNode = JsonNode.Parse(jsonContent);

if (jsonNode is JsonObject jsonObject)
{
    Console.WriteLine("It's a JSON object:");
    Console.WriteLine(jsonObject);
}
else if (jsonNode is JsonArray jsonArray)
{
    Console.WriteLine("It's a JSON array:");
    Console.WriteLine(jsonArray);
}
else
{
    Console.WriteLine("It's a primitive JSON value:");
    Console.WriteLine(jsonNode);
}
