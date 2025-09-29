using System.Text.Json.Nodes;

namespace Dr.ToolDiscoveryService.Abstractions;

public enum ToolParameterType
{
    String,
    StringArray,
    Int,
    IntArray,
    Decimal,
    DecimalArray
}


public class ToolDefinitionBuilder
{
    private string? _name = null;
    private string? _description = null;
    private JsonObject? _parameters = null;

    public ToolDefinitionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ToolDefinitionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ToolDefinitionBuilder WithParameters(Action<ToolDefinitionParameterBuilder> builder)
    {
        var paramBuilder = new ToolDefinitionParameterBuilder();
        builder(paramBuilder);
        _parameters = paramBuilder.Build();
        return this;
    }

    public ToolDefinition Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Tool name is required");

        if (string.IsNullOrWhiteSpace(_description))
            throw new InvalidOperationException("Tool description is required");

        var definition = new JsonObject
        {
            ["name"] = _name.ToLowerInvariant(),
            ["description"] = _description
        };

        if (_parameters != null)
            definition.Add("parameters", _parameters);

        return new ToolDefinition(_name, _description, definition);
    }
}


public class ToolDefinitionParameterBuilder
{
    private readonly JsonObject _properties = [];
    private readonly HashSet<string> _required = [];

    public ToolDefinitionParameterBuilder WithProperty(string name, Action<ToolDefinitionPropertyBuilder> builder)
    {
        var propBuilder = new ToolDefinitionPropertyBuilder();
        builder(propBuilder);
        _properties.Add(name, propBuilder.Build());
        return this;
    }

    public ToolDefinitionParameterBuilder WithRequiredProperty(string name, Action<ToolDefinitionPropertyBuilder> builder)
    {
        WithProperty(name, builder);
        _required.Add(name);
        return this;
    }

    public JsonObject Build()
    {
        var parameters = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = _properties
        };

        if (_required.Any())
        {
            var requiredArray = new JsonArray();
            foreach (var required in _required)
                requiredArray.Add(required);
            parameters["required"] = requiredArray;
        }

        return parameters;
    }
}

public class ToolDefinitionPropertyBuilder
{
    private string? _type = null;
    private string? _description = null;
    private JsonArray _enumValues = [];
    private JsonObject? _properties = null;

    public ToolDefinitionPropertyBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public ToolDefinitionPropertyBuilder WithType(ToolParameterType type)
    {
        _type = type switch
        {
            ToolParameterType.String => "string",
            ToolParameterType.StringArray => "array",
            ToolParameterType.Int => "integer",
            ToolParameterType.IntArray => "array",
            ToolParameterType.Decimal => "number",
            ToolParameterType.DecimalArray => "array",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        return this;
    }

    public ToolDefinitionPropertyBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ToolDefinitionPropertyBuilder WithEnum(params string[] values)
    {
        _enumValues = new JsonArray();
        values.ToList().ForEach(v => _enumValues.Add(v));
        return this;
    }

    public ToolDefinitionPropertyBuilder WithObjectProperties(Action<ToolDefinitionParameterBuilder> builder)
    {
        var paramBuilder = new ToolDefinitionParameterBuilder();
        builder(paramBuilder);
        var result = paramBuilder.Build();

        if (result["properties"] is JsonObject props)
            _properties = JsonNode.Parse(props.ToJsonString())?.AsObject();

        return this;
    }

    public JsonObject Build()
    {
        var node = new JsonObject
        {
            ["type"] = _type,
            ["description"] = _description
        };

        if (_enumValues.Any())
            node["enum"] = _enumValues;

        if (_properties != null)
            node["properties"] = _properties;

        return node;
    }
}
