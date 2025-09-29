using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Dr.FakerAnalytics.Cli.Tools;

/// <summary>
/// Executes any tool registered with the Tool Discovery Service.
/// </summary>
public class ToolExecutor
{
    private const string jsonTrue = "true";
    private const string jsonFalse = "false";
    private const string jsonNull = "null";
    private const string applicationJson = "application/json";

    private HttpClient _httpClient = new()
    {
        DefaultRequestHeaders = {
            Accept = { new MediaTypeWithQualityHeaderValue(applicationJson) }
        }
    };

    // TODO: Maybe execute tool by name -> fetch from disco?
    public async Task<JsonNode> ExecuteAsync(Tool tool, JsonNode jsonParameters, CancellationToken cancellationToken) =>
        await (tool.ToolRoute.HttpRequestMethod switch
        {
            HttpRequestMethod.Get => ExecuteGetAsync(tool, jsonParameters, cancellationToken),
            HttpRequestMethod.Post => ExecutePostAsync(tool, jsonParameters, cancellationToken),
            HttpRequestMethod.Put => ExecutePutAsync(tool, jsonParameters, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(ToolRoute.HttpRequestMethod))
        });

    private async Task<JsonNode> ExecuteGetAsync(Tool tool, JsonNode jsonParameters, CancellationToken cancellationToken)
    {

        try
        {
            var queryString = ToQueryString(jsonParameters);
            var response = await _httpClient.GetAsync($"{tool.ToolRoute.Uri}{queryString}");

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(jsonContent))
                return new JsonObject();

            // TODO: Nullable here?
            return JsonNode.Parse(jsonContent)!;
        }
        catch
        {
            //  TODO: Logging.
            throw;
        }
    }

    private async Task<JsonNode> ExecutePostAsync(Tool tool, JsonNode jsonParameters, CancellationToken cancellationToken)
    {
        try
        {
            var content = JsonContent.Create(jsonParameters);
            var response = await _httpClient.PostAsync(tool.ToolRoute.Uri, content, cancellationToken);

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

    private async Task<JsonNode> ExecutePutAsync(Tool tool, JsonNode jsonParameters, CancellationToken cancellationToken)
    {
        try
        {
            var content = JsonContent.Create(jsonParameters);
            var response = await _httpClient.PutAsync(tool.ToolRoute.Uri, content, cancellationToken);

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

    private string ToQueryString(JsonNode jsonParameters)
    {
        // TODO: What should we do here?
        if (jsonParameters is not JsonObject jsonObject)
            return string.Empty;


        var queryStrings = new Dictionary<string, string>();

        // Find requested parameters.
        foreach (var parameter in jsonObject)
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
