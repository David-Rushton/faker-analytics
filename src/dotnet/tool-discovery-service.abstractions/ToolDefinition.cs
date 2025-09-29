using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Dr.ToolDiscoveryService.Abstractions;

public class ToolDefinition
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        IndentSize = 2,
        IndentCharacter = ' '
    };

    [JsonConstructor]
    public ToolDefinition(string name, string description, JsonObject definition)
    {
        Name = name.ToLowerInvariant();
        Description = description;
        Definition = definition;
    }

    public string Name { get; }
    public string Description { get; }

    [JsonPropertyName("definition")]
    public JsonObject Definition { get; }

    public string ToJson(bool pretty = false) =>
        pretty
            ? JsonSerializer.Serialize(Definition, _jsonOptions).Replace("\r", string.Empty)
            : Definition.ToJsonString();
}
