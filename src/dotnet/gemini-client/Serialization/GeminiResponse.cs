namespace Dr.GeminiClient.Serialization;

/// <summary>
///   A Gemini <see href="https://ai.google.dev/api/generate-content?_gl=1*1higdbb*_up*MQ..*_ga*NDcwNTIxNDg0LjE3NTkxMzc4NTg.*_ga_P1DBVKWT6V*czE3NTkxMzc4NTgkbzEkZzAkdDE3NTkxMzc4NTgkajYwJGwwJGgxMzc2MjA5MDcx#v1beta.GenerateContentResponse">response</see>.
/// </summary>
internal class GeminiResponse
{
    public required List<Candidate> Candidates { get; init; }
    public required UsageMetadata UsageMetadata { get; init; }
    public required string ModelVersion { get; init; }
    public required string ResponseId { get; init; }

    internal Candidate GetPreferredCandidate()
    {
        if (Candidates.Count == 0)
            return Candidate.Empty();

        return Candidates
            .OrderBy(c => c.GetFinishReasonRank())
            .First();
    }
}

/// <summary>
///   A response can contain multiple candidates.
/// </summary>
internal class Candidate
{
    public required Content Content { get; init; }
    public required int Index { get; init; }
    public string? FinishReason { get; init; }


    /// <summary>
    ///    Ranks the candidate by its finish reason.  Where lower finish reasons should be preferred.
    //    <seeAlso>
    //      https://ai.google.dev/api/generate-content?_gl=1*wql7og*_up*MQ..*_ga*NDcwNTIxNDg0LjE3NTkxMzc4NTg.*_ga_P1DBVKWT6V*czE3NTkxMzc4NTgkbzEkZzAkdDE3NTkxMzc4NTgkajYwJGwwJGgxMzc2MjA5MDcx#FinishReason
    //    </seeAlso>
    /// </summary>
    internal int GetFinishReasonRank() =>
        FinishReason switch
        {
            null => 0,
            "STOP" => 1,
            _ => 2
        };

    internal static Candidate Empty() =>
        new Candidate
        {
            Content = new()
            {
                Role = string.Empty,
                Parts = new()
            },
            Index = 0,
            FinishReason = null
        };
}

internal class Content
{
    public required string Role { get; init; }
    public required List<Part> Parts { get; init; }
}

internal class Part
{
    public string? Text { get; init; }
    public bool Thought { get; init; }
    public string? ThoughtSignature { get; init; }

    public FunctionCall? FunctionCall { get; init; }
}

internal class FunctionCall
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


internal class UsageMetadata
{
    public int? PromptTokenCount { get; init; } = 0;
    public int? CandidatesTokenCount { get; init; } = 0;
    public int? ThoughtsTokenCount { get; init; } = 0;
    public int? TotalTokenCount { get; init; } = 0;
}
