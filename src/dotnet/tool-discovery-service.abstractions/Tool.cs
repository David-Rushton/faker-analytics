
namespace Dr.ToolDiscoveryService.Abstractions;

public class Tool
{
    public string Name => ToolDefinition.Name.ToLowerInvariant();
    public required ToolDefinition ToolDefinition { get; init; }
    public required ToolRoute ToolRoute { get; init; }
}
