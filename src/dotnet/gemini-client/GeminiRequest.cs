using Dr.ToolDiscoveryService.Abstractions;

namespace Dr.GeminiClient;

internal class GeminiRequest
{
    public required List<GeminiContent> Contents { get; init; }
    public required GenerationConfig GenerationConfig { get; set; }
    public required List<Tool> Tools { get; set; }
}

public class GenerationConfig
{
    public required ThinkingConfig ThinkingConfig { get; set; }
}

public class ThinkingConfig
{
    public required int ThinkingBudget { get; set; } = -1;
    public required bool IncludeThoughts { get; set; } = true;
}

internal class GeminiContent
{
    public required GeminiRole Role { get; init; }
    public required List<GeminiContentPart> Parts { get; init; }
}

internal class GeminiContentPart
{
    public string? Text { get; init; }
    public GeminiFunctionCall? FunctionCall { get; init; }
    public bool Thought { get; set; }
    public string? ThoughtSignature { get; init; }
}

public class GeminiFunctionCall
{
    public required string Name { get; init; }
    public required Dictionary<string, object> Args { get; init; }

    public override string ToString()
    {
        var flatArgs = string.Join(
            ",",
            Args.Select(kvp => $"[{kvp.Key}: {kvp.Value}]"));

        return $"{{ FunctionCall {{ Name = {Name}, Args = {flatArgs} }} }}";
    }
}

public enum GeminiRole { User, System, Model }
internal enum GeminiPart { Text }
