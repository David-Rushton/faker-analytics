using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dr.GeminiClient;

public class Response
{ }

public class TextResponse : Response
{
    public required string Text { get; init; }
}

public class ThoughtResponse : Response
{
    public required string Thought { get; init; }
    public required string ThoughtSignature { get; init; }
}

public class FunctionCallResponse : Response
{
    public required string Name { get; init; }
    public required Dictionary<string, object> Args { get; init; }

    public JsonNode GetJsonArgs()
    {
        var json = JsonSerializer.Serialize(Args) ?? "{}";
        var jsonObj = JsonNode.Parse(json)!;

        return jsonObj;
    }
}
