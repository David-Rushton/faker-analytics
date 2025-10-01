using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Dr.ChartingService.Repositories;

public class WebpageRepository
{
    private readonly ConcurrentDictionary<Guid, string> _webpages = new();

    public void Add(Guid webpageId, string webpage)
    {
        if (webpage.StartsWith("```"))
            webpage = webpage[3..];

        if (webpage.EndsWith("```"))
            webpage = webpage[..^3];

        _webpages[webpageId] = webpage;
    }

    public bool TryGet(Guid webPageId, [NotNullWhen(returnValue: true)] out string? webPage) =>
        _webpages.TryGetValue(webPageId, out webPage);
}
