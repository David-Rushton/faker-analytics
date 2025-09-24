using System.Text.Json.Serialization;

namespace Dr.Gemini;

public class GeminiTools
{
    public required List<GeminiTool>? FunctionDeclarations { get; init; }
}

public class GeminiTool
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required GeminiToolParameters Parameters { get; init; }
}

public class GeminiToolParameters
{
    public required string Type { get; init; }
    public required Dictionary<string, GeminiToolParameter> Properties { get; init; }
    public required List<string> Required { get; init; }
}

public class GeminiToolParameter
{
    [JsonConverter(typeof(JsonStringOrArrayConverter))]
    public required string Type { get; set; }
    public string? Format { get; set; }
    public string? Pattern { get; set; }
}
