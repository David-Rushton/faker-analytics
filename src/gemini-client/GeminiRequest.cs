using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Dr.Gemini;

internal class GeminiRequest
{
    public required List<GeminiContent> Contents { get; init; }
    public required GenerationConfig GenerationConfig { get; set; }
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
    public required string Text { get; init; }
    public bool Thought { get; set; }
}

public enum GeminiRole { User, System, Model }
internal enum GeminiPart { Text }
