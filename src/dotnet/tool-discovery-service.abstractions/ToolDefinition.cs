using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dr.ToolDiscoveryService.Abstractions;

public class ToolDefinition(string name, string description, JsonObject definition)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        IndentSize = 2,
        IndentCharacter = ' '
    };

    public string Name => name;
    public string Description => description;

    public string ToJson(bool pretty = false) =>
        pretty
            ? JsonSerializer.Serialize(definition, _jsonOptions).Replace("\r", string.Empty)
            : definition.ToJsonString();
}
