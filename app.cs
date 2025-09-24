using System.Text.Json.Nodes;

string jsonString = """
{
    "name": "David",
    "age": 46,
    "numbers": [1, 2, 3],
    "words": ["the", "mdu"],
    "employer": {
        "name": "Trayport"
    }
}
""";

JsonObject jsonObject = (JsonObject)JsonNode.Parse(jsonString)!;

// Console.WriteLine($"Name: {jsonObject["name"]}");
// Console.WriteLine($"Age: {jsonObject["age"]}");
// Console.WriteLine($"Age: {jsonObject["employer"]!["name"]}");
// Console.WriteLine($"{jsonObject}");



foreach (var p in jsonObject)
{
    // Console.WriteLine(p);
    Console.Write(p.Key);
    Console.Write(" | ");
    Console.WriteLine(p.Value);
    Console.WriteLine(p.Value.GetValueKind());

    if (p.Value.GetValueKind() == System.Text.Json.JsonValueKind.Array)
    {
            Console.WriteLine($"  {string.Join(",", p.Value.AsArray())}");
    }


    Console.WriteLine();
}
