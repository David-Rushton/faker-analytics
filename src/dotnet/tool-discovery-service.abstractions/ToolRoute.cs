namespace Dr.ToolDiscoveryService.Abstractions;

public enum HttpRequestMethod
{
    Get,
    Post,
    Put
}

public class ToolRoute
{
    public required HttpRequestMethod HttpRequestMethod { get; init; }
    public required Uri Uri { get; init; }
}
