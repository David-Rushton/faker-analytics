using Dr.GeminiClient.Extensions;
using Dr.GeminiClient.Serialization;
using Dr.ToolDiscoveryService.Abstractions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
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
    private readonly List<Content> _history = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
    ///   <para>
    ///     Ask the model a question.
    ///   </para>
    ///   <para>
    ///     Sends a request, and requests context.
    ///   </para>
    /// </summary>
    /// <param name="request">What do you want to know>?</param>
    /// <returns>
    ///   It take a while for the model to formulate a complete response.  It may choose to send you
    ///   intermediate updates as it progresses.
    /// </returns>
    public async IAsyncEnumerable<Response> Ask(string request)
    {
        _history.Clear();
        _history.Add(new Content
        {
            Role = "user",
            Parts =
            [
                new Part { Text = request }
            ]
        });

        await foreach (var response in SendRequest())
            yield return response;
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
    public async IAsyncEnumerable<Response> ReplyWithFunctionResult(Tool tool, JsonNode result)
    {
        if (_history.Count == 0 || _history.Last().Parts.Count == 0)
            throw new InvalidOperationException("ReplyWithFunctionResult should only be called when replying to a function call");

        Part last = _history.Last()!.Parts.Last()!;

        if (last.FunctionCall is null)
            throw new InvalidOperationException("ReplyWithFunctionResult should only be called when replying to a function call");


        Struct protoStruct = result.ToProtobufStruct();


        _history.Add(new()
        {
            Role = "user",  // too;?
            Parts = [new()
            {
                FunctionResponse = new FunctionResponse
                {
                    Name = tool.Name,
                    Response = protoStruct
                }
            }]
        });

        await foreach (var response in SendRequest())
            yield return response;
    }


    private async IAsyncEnumerable<Response> SendRequest()
    {
        var httpResponse = await MakeRequest();
        await foreach (var response in ProcessResponses(httpResponse))
            yield return response;

        // Make a request to Gemini.
        async Task<HttpResponseMessage> MakeRequest()
        {
            var jsonContent = new JsonObject
            {
                ["contents"] = GetHistoryAsJson(),
                ["system_instruction"] = new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["text"] = SystemInstruction
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
                    ["functionDeclarations"] = new JsonArray(Tools.Select(t => JsonNode.Parse(t.ToolDefinition.Definition.ToJsonString())!).ToArray())
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Content = JsonContent.Create(jsonContent)
            };
            httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

            var httpResponse = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine(await httpResponse.Content.ReadAsStringAsync());



            httpResponse.EnsureSuccessStatusCode();

            return httpResponse;
        }

        // Convert history to JSON.
        JsonArray GetHistoryAsJson()
        {
            var result = new JsonArray();

            foreach (var item in _history)
                result.Add(JsonNode.Parse(JsonSerializer.Serialize(item, _jsonOptions)));

            return result;
        }

        // Process a response from Gemini.
        async IAsyncEnumerable<Response> ProcessResponses(HttpResponseMessage httpResponse)
        {
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
                    if (part.Thought ?? false)
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
                        _history.Add(new() { Role = "model", Parts = [part] });

                        yield return new FunctionCallResponse
                        {
                            Name = part.FunctionCall.Name,
                            Args = part.FunctionCall.Args
                        };

                        continue;
                    }

                    if (string.IsNullOrEmpty(part.Text))
                        throw new InvalidOperationException("Expected a Gemini part to contain text.");

                    _history.Add(new() { Role = "model", Parts = [part] });

                    yield return new TextResponse { Text = part.Text };
                }
            }

            yield break;
        }
    }
}
