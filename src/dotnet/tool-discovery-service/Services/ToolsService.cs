namespace Dr.ToolDiscoveryService.Services;

using ToolRegistration = (DateTimeOffset expires, Tool tool);

public class ToolsServiceOptions
{
    public required int ToolExpiryIntervalInSeconds { get; init; } = (60 * 5);
}

/// <summary>
///   <para>
///     An in-memory tool registration service.
///   </para>
///   <para>
///     Tools registered with this discovery service are tracked here.
///   </para>
/// </summary>
/// <param name="logger"></param>
/// <param name="options"></param>
public class ToolsService(
    ILogger<ToolsService> logger,
    IOptions<ToolsServiceOptions> options,
    TimeProvider timeProvider)
{
    private readonly Dictionary<string, ToolRegistration> _registeredTools = new();

    public IEnumerable<Tool> List()
    {
        foreach (var (_, (_, tool)) in _registeredTools)
            yield return tool;
    }

    /// <summary>
    /// Try retrieve a registered tool
    /// </summary>
    public bool TryGet(string name, [NotNullWhen(returnValue: true)] out Tool? tool)
    {
        if (_registeredTools.TryGetValue(name, out var registration))
        {
            logger.LogInformation("Discovery service found tool {toolName}.", registration.tool.Name);

            tool = registration.tool;
            return true;
        }

        logger.LogInformation("Discovery service could not find tool {toolName}.", registration.tool.Name);

        tool = null;
        return false;
    }

    /// <summary>
    ///   <para>
    ///     Upserts the current <see cref="ToolDefinition"/>.
    ///   </para>
    ///   <para>
    ///     <see cref="ToolDefinition"/>s must be periodically update to remain available.
    ///   </para>
    /// </summary>
    /// <param name="toolDefinition"></param>
    public void AddOrUpdate(Tool tool)
    {
        logger.LogInformation("Registering tool {toolName} with discovery service.", tool.Name);

        _registeredTools[tool.Name] = (
            expires: timeProvider.GetUtcNow().AddSeconds(options.Value.ToolExpiryIntervalInSeconds),
            tool
        );
    }

    /// <summary>
    /// Should be called periodically to prune tools that are not currently available.
    /// </summary>
    public void RemoveExpiredTools()
    {
        HashSet<string> expired = new();
        foreach (var (name, (expires, _)) in _registeredTools)
        {
            if (expires < timeProvider.GetUtcNow())
            {
                expired.Add(name);
                logger.LogInformation("Tool {toolName} registration has expired.  The tool will be removed from discovery service.", name);
            }
        }

        foreach (var item in expired)
            _registeredTools.Remove(item);
    }
}
