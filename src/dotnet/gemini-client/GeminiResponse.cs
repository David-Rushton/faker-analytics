namespace Dr.GeminiClient;

internal class GeminiResponse
{
    public required List<GeminiResponseCandidate> Candidates { get; init; }
    public required GeminiUsageMetadata UsageMetadata { get; init; }
    public required string ModelVersion { get; init; }
    public required string ResponseId { get; init; }
}

internal class GeminiCandidate
{
    public required GeminiContent Contents { get; init; }
    public required int Index { get; init; }
    public string? FinishReason { get; init; }
}

internal class GeminiResponseCandidate
{
    public required GeminiContent Content { get; init; }
    public required int Index { get; init; }
    public string? FinishReason { get; init; }
}

internal class GeminiUsageMetadata
{
    public int? PromptTokenCount { get; init; } = 0;
    public int? CandidatesTokenCount { get; init; } = 0;
    public int? ThoughtsTokenCount { get; init; } = 0;
    public int? TotalTokenCount { get; init; } = 0;
}
