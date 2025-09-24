using System.Reflection.Metadata.Ecma335;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Net.Mime;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dr.ToolDiscoveryService.Abstractions;

public class Tools
{
    public List<Tool> Items { get; } = new();

    public void Add(Tool tool) =>
        Items.Add(tool);

    public string ToJson(bool pretty = false) =>
        $"[{string.Join(",", Items.Select(t => t.ToolDefinition.ToJson(pretty)))}]";
}

public class Tool
{
    private const string jsonTrue = "true";
    private const string jsonFalse = "false";
    private const string jsonNull = "null";
    private const string applicationJson = "application/json";

    private HttpClient _httpClient = new()
    {
        DefaultRequestHeaders = {
            Accept = { new MediaTypeWithQualityHeaderValue("applicationJson") }
        }
    };

    private JsonSerializerOptions _jsonOptions = new();

    public string Name => ToolDefinition.Name;
    public required ToolDefinition ToolDefinition { get; init; }
    public required ToolRoute ToolRoute { get; init; }

    public async Task<JsonObject> ExecuteAsync(JsonObject jsonParameters, CancellationToken cancellationToken) =>
        await ((ToolRoute.HttpRequestMethod) switch
        {
            HttpRequestMethod.Get => ExecuteGetAsync(jsonParameters, cancellationToken),
            HttpRequestMethod.Post => ExecutePostAsync(jsonParameters, cancellationToken),
            HttpRequestMethod.Put => ExecutePutAsync(jsonParameters, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(ToolRoute.HttpRequestMethod))
        });

    private async Task<JsonObject> ExecuteGetAsync(JsonObject jsonParameters, CancellationToken cancellationToken)
    {

        try
        {
            var queryString = ToQueryString(jsonParameters);
            var response = await _httpClient.GetAsync($"{ToolRoute.Uri}{queryString}");

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(jsonContent))
                return new JsonObject();

            return (JsonObject?)JsonNode.Parse(jsonContent) ?? new JsonObject();
        }
        catch
        {
            // TODO: Logging.
            throw;
        }
    }

    private async Task<JsonObject> ExecutePostAsync(JsonObject jsonParameters, CancellationToken cancellationToken)
    {
        try
        {
            var content = JsonContent.Create(jsonParameters);
            var response = await _httpClient.PostAsync(ToolRoute.Uri, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(jsonContent))
                return new JsonObject();

            return (JsonObject?)JsonNode.Parse(jsonContent) ?? new JsonObject();
        }
        catch
        {
            // TODO: Logging.
            throw;
        }
    }

    private async Task<JsonObject> ExecutePutAsync(JsonObject jsonParameters, CancellationToken cancellationToken)
    {
        try
        {
            var content = JsonContent.Create(jsonParameters);
            var response = await _httpClient.PutAsync(ToolRoute.Uri, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(jsonContent))
                return new JsonObject();

            return (JsonObject?)JsonNode.Parse(jsonContent) ?? new JsonObject();
        }
        catch
        {
            // TODO: Logging.
            throw;
        }
    }

    private string ToQueryString(JsonObject jsonParameters)
    {
        var queryStrings = new Dictionary<string, string>();

        // Find requested parameters.
        foreach (var parameter in jsonParameters)
        {
            string key = parameter.Key;
            string value = string.Empty;

            if (parameter.Value is not null)
                value = (parameter.Value.GetValueKind()) switch
                {
                    System.Text.Json.JsonValueKind.Array => string.Join(",", parameter.Value.AsArray()),
                    System.Text.Json.JsonValueKind.String => parameter.Value.ToString(),
                    System.Text.Json.JsonValueKind.Number => parameter.Value.ToString(),
                    System.Text.Json.JsonValueKind.True => jsonTrue,
                    System.Text.Json.JsonValueKind.False => jsonFalse,
                    System.Text.Json.JsonValueKind.Null => jsonNull,
                    _ => throw new ArgumentOutOfRangeException($"GET tools do not support parameter types of {parameter.Value.GetValueKind()}.")
                };

            if (queryStrings.ContainsKey(key))
            {
                queryStrings[key] += $",{value}";
                continue;
            }

            queryStrings[key] = value;
        }

        // Convert to a query string.
        if (queryStrings.Count == 0)
            return string.Empty;

        return "?" + string.Join("&", queryStrings.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }
}
