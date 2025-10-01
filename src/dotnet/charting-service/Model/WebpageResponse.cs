using Microsoft.AspNetCore.Mvc.Routing;

namespace Dr.ChartingService.Model;

public class WebpageResponse
{
    public required Guid WebpageId { get; init; }
    public string Url
    {
        get
        {
            var host = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5254";
            return $"{host}/api/webpages/{WebpageId}";
        }
    }
}
