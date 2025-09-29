using Dr.GeminiClient.Serialization;
using Dr.ToolDiscoveryService.Abstractions;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dr.GeminiClient;

public partial class Conversation
{
    internal static string Url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:streamGenerateContent?alt=sse";

    /// <summary>
    /// Start a new conversation.
    /// </summary>
    public static Conversation Create(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentOutOfRangeException("An API key is required to talk to Gemini.");

        return new Conversation(Url, apiKey);
    }
}

/// <summary>
/// Converse with GenAI.
/// </summary>
public partial class Conversation
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly GeminiResponse? _lastResponse;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal Conversation(string url, string apiKey)
    {
        _url = url;
        _httpClient = new()
        {
            DefaultRequestHeaders =
            {
                { "X-Goog-Api-Key", apiKey }
            }
        };
    }

    /// <summary>
    ///   Guide the behavior of the GenAI model with system instructions.
    /// </summary>
    public string SystemInstruction { get; set; } = string.Empty;

    /// <summary>
    ///   <para>
    ///     The tools available to the GenAI model.
    ///   </para>
    ///   <remarks>
    ///     If tools are provided you should explicitly state in the system instructions if
    ///     usage is optional or compulsory.  The default behaviour is inconsistent.
    ///   </remarks>
    /// </summary>
    public List<Tool> Tools { get; set; } = [];

    /// <summary>
    ///   Ask the model a question.
    /// </summary>
    /// <param name="request">What do you want to know>?</param>
    /// <returns>
    ///   It take a while for the model to formulate a complete response.  It may choose to send you
    ///   intermediate updates as it progresses.
    /// </returns>
    public async IAsyncEnumerable<Response> Ask(string request)
    {
        var jsonContent = new JsonObject
        {
            ["contents"] = new JsonObject
            {
                ["role"] = "user",
                ["parts"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["text"] = request
                    }
                }
            },
            ["generationConfig"] = new JsonObject
            {
                ["thinkingConfig"] = new JsonObject
                {
                    ["thinkingBudget"] = -1,
                    ["includeThoughts"] = true
                }
            },
            ["tools"] = new JsonObject
            {
                ["functionDeclarations"] = new JsonArray(Tools.Select(t => t.ToolDefinition.Definition).ToArray())
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Content = JsonContent.Create(jsonContent)
        };
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

        var httpResponse = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        httpResponse.EnsureSuccessStatusCode();

        using var reader = new StreamReader(await httpResponse.Content.ReadAsStreamAsync());

        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            if (line.StartsWith("data: "))
                line = line[6..];

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(line, _jsonOptions);

            if (geminiResponse is null || geminiResponse.Candidates.Count() == 0)
                throw new InvalidOperationException("Expected a Gemini response.");

            var candidate = geminiResponse.GetPreferredCandidate();

            foreach (var part in candidate.Content.Parts)
            {
                if (part.Thought)
                {
                    if (string.IsNullOrEmpty(part.Text))
                        throw new InvalidOperationException("Expected a Gemini part to contain text.");

                    yield return new ThoughtResponse
                    {
                        Thought = part.Text,
                        ThoughtSignature = part.ThoughtSignature ?? string.Empty
                    };

                    continue;
                }

                if (part.FunctionCall is not null)
                {
                    yield return new FunctionCallResponse
                    {
                        Name = part.FunctionCall.Name,
                        Args = part.FunctionCall.Args
                    };

                    continue;
                }

                if (string.IsNullOrEmpty(part.Text))
                    throw new InvalidOperationException("Expected a Gemini part to contain text.");

                yield return new TextResponse { Text = part.Text };
            }
        }

        yield break;
    }

    /// <summary>
    ///   Continue a conversation, retaining context.
    /// </summary>
    public IEnumerable<Response> Reply()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///   When requested; reply with the result of a function call.
    /// </summary>
    public IEnumerable<Response> ReplyWithFunctionResult()
    {
        throw new NotImplementedException();
    }
}
