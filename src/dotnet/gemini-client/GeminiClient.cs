using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dr.Gemini;

public class GeminiClientOptions
{
    internal static string Url => "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:streamGenerateContent?alt=sse";
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
    public List<GeminiTool>? Tools { get; set; }


    public async IAsyncEnumerable<(bool isThought, string? text, GeminiFunctionCall? functionCall)> GetResponseStream(string prompt)
    {
        logger.LogInformation("GeminiClient url: {url}", GeminiClientOptions.Url);
        logger.LogInformation("GeminiClient initialised with API key: {redactedKey}", options.Value.ApiKey[..4] + "********");

        var request = new GeminiRequest
        {
            Contents = new List<GeminiContent>
            {
                new()
                {
                    Role = GeminiRole.User,
                    Parts = new List<GeminiContentPart>
                    {
                        new()
                        {
                            // Part = GeminiPart.Text,
                            Text = prompt
                        }
                    }
                }
            },
            GenerationConfig = new()
            {
                ThinkingConfig = new()
                {
                    ThinkingBudget = -1,
                    IncludeThoughts = true
                }
            },
            Tools = new()
            {
                FunctionDeclarations = this.Tools
            }
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

    public void AddTools(string tools)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            // Add the JsonStringEnumConverter to serialize enums as strings
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // HACK!
        var toolsObj = JsonSerializer.Deserialize<IEnumerable<GeminiTool>>(tools, jsonOptions);
        Tools = toolsObj?.ToList() ?? [];
    }
}
