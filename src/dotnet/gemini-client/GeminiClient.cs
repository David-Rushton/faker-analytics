using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dr.ToolDiscoveryService.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dr.GeminiClient;

public class GeminiClientOptions
{
    internal static string Url => "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:streamGenerateContent?alt=sse";

    [MinLength(32)]
    public required string ApiKey { get; init; }
}

public class GeminiClient(ILogger<GeminiClientOptions> logger, IOptions<GeminiClientOptions> options)
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(GeminiClientOptions.Url),
        DefaultRequestHeaders =
        {
            { "X-Goog-Api-Key", options.Value.ApiKey }
        }
    };

    // HACK!
    public List<Tool> Tools { get; set; } = new();

    public async IAsyncEnumerable<(bool isThought, string? text, GeminiFunctionCall? functionCall)> GetResponseStream(string prompt)
    {
        logger.LogInformation("GeminiClient url: {url}", GeminiClientOptions.Url);
        logger.LogInformation("GeminiClient xxx: {url}", GeminiClientOptions.Url);
        logger.LogInformation("GeminiClient api key");

        var request = new JsonObject
        {
            ["contents"] = new JsonObject
            {
                ["role"] = GeminiRole.User.ToString(),
                ["parts"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["text"] = prompt
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
            ["tools"] = new JsonArray(Tools.Select(t => t.ToolDefinition.Definition).ToArray())
        };


        var jsonOptions = new JsonSerializerOptions
        {
            // Add the JsonStringEnumConverter to serialize enums as strings
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Console.WriteLine("--------------------------------");
        // Console.WriteLine(JsonSerializer.Serialize(request, jsonOptions));
        // Console.WriteLine("--------------------------------");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri: string.Empty)
        {
            Content = JsonContent.Create(request, options: jsonOptions)
        };
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

        var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.Error.WriteLine(await response.Content.ReadAsStringAsync());
        }

        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        string? line;

        var sb = new StringBuilder();

        while ((line = await reader.ReadLineAsync()) != null)
        {
            logger.LogInformation("resp json:\n{json}\n", line ?? string.Empty);

            sb.AppendLine(line);
            sb.AppendLine("");

            if (string.IsNullOrEmpty(line))
                continue;

            // if (line == "" or "data: [DONE]")
            //     continue;

            if (line.StartsWith("data: "))
                line = line[6..];

            GeminiResponse? geminiResponse;
            try
            {
                geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(line!, jsonOptions);
            }
            catch (JsonException e)
            {
                Console.Error.WriteLine($"Failed to deserialize Gemini response: {e}");
                logger.LogError(e, "Failed to deserialize Gemini response: {Line}", line);
                continue;
            }

            if (geminiResponse?.Candidates is null)
                continue;

            foreach (var candidate in geminiResponse.Candidates)
            {
                var role = candidate.Content.Role;

                foreach (var part in candidate.Content.Parts)
                {

                    yield return (part.Thought, part.Text, part.FunctionCall);
                    // if (part.Part == GeminiPart.Text && !string.IsNullOrWhiteSpace(part.Text))
                    // {
                    // }
                }
            }
        }

        // Console.WriteLine("--------------------------------");
        // Console.WriteLine(sb.ToString());
        // Console.WriteLine("--------------------------------");

        yield break;
    }
}
