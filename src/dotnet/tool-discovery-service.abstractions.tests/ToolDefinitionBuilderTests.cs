using System.Text.Json.Nodes;
using Dr.ToolDiscoveryService.Abstractions;

namespace Dr.ToolDiscoveryService.Abstractions.Tests;

public class ToolDefinitionBuilderTests
{
    [Fact]
    public void Build_WithNameAndDescription_CreatesValidToolDefinition()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var tool = builder
            .WithName("test-tool")
            .WithDescription("A test tool for unit testing")
            .Build();

        // Assert
        Assert.Equal("test-tool", tool.Name);
        Assert.Equal("A test tool for unit testing", tool.Description);

        var json = tool.ToJson();
        var jsonNode = JsonNode.Parse(json)!;
        Assert.Equal("test-tool", jsonNode["name"]!.GetValue<string>());
        Assert.Equal("A test tool for unit testing", jsonNode["description"]!.GetValue<string>());
    }

    [Fact]
    public void Build_WithoutName_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.WithDescription("A test tool").Build());
        Assert.Equal("Tool name is required", exception.Message);
    }

    [Fact]
    public void Build_WithoutDescription_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.WithName("test-tool").Build());
        Assert.Equal("Tool description is required", exception.Message);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.WithName("").WithDescription("A test tool").Build());
        Assert.Equal("Tool name is required", exception.Message);
    }

    [Fact]
    public void Build_WithEmptyDescription_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.WithName("test-tool").WithDescription("").Build());
        Assert.Equal("Tool description is required", exception.Message);
    }

    [Fact]
    public void Build_WithSimpleParameters_CreatesValidToolDefinition()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var tool = builder
            .WithName("calculator")
            .WithDescription("A simple calculator tool")
            .WithParameters(p => p
                .WithProperty("operation", prop => prop
                    .WithType(ToolParameterType.String)
                    .WithDescription("The mathematical operation to perform")
                    .WithEnum("add", "subtract", "multiply", "divide"))
                .WithProperty("numbers", prop => prop
                    .WithType(ToolParameterType.DecimalArray)
                    .WithDescription("The numbers to operate on")))
            .Build();

        // Assert
        var json = tool.ToJson();
        var jsonNode = JsonNode.Parse(json)!;

        var parameters = jsonNode["parameters"]!.AsObject();
        Assert.Equal("object", parameters["type"]!.GetValue<string>());

        var properties = parameters["properties"]!.AsObject();
        Assert.Contains("operation", properties.Select(kv => kv.Key));
        Assert.Contains("numbers", properties.Select(kv => kv.Key));

        var operationProp = properties["operation"]!.AsObject();
        Assert.Equal("string", operationProp["type"]!.GetValue<string>());
        Assert.Equal("The mathematical operation to perform", operationProp["description"]!.GetValue<string>());

        var enumArray = operationProp["enum"]!.AsArray();
        Assert.Equal(4, enumArray.Count);
        Assert.Contains("add", enumArray.Select(e => e!.GetValue<string>()));
        Assert.Contains("subtract", enumArray.Select(e => e!.GetValue<string>()));
    }

    [Fact]
    public void Build_WithRequiredParameters_CreatesValidToolDefinition()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var tool = builder
            .WithName("file-reader")
            .WithDescription("Reads content from a file")
            .WithParameters(p => p
                .WithRequiredProperty("filepath", prop => prop
                    .WithType(ToolParameterType.String)
                    .WithDescription("The path to the file to read"))
                .WithProperty("encoding", prop => prop
                    .WithType(ToolParameterType.String)
                    .WithDescription("The file encoding (optional)")))
            .Build();

        // Assert
        var json = tool.ToJson();
        var jsonNode = JsonNode.Parse(json)!;

        var parameters = jsonNode["parameters"]!.AsObject();
        var required = parameters["required"]!.AsArray();

        Assert.Single(required);
        Assert.Equal("filepath", required[0]!.GetValue<string>());
    }

    [Fact]
    public void Build_WithNestedObjectProperties_CreatesValidToolDefinition()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var tool = builder
            .WithName("config-tool")
            .WithDescription("Tool with nested configuration")
            .WithParameters(p => p
                .WithProperty("config", prop => prop
                    .WithType("object")
                    .WithDescription("Configuration object")
                    .WithObjectProperties(nested => nested
                        .WithProperty("host", hostProp => hostProp
                            .WithType(ToolParameterType.String)
                            .WithDescription("Server host"))
                        .WithProperty("port", portProp => portProp
                            .WithType(ToolParameterType.Int)
                            .WithDescription("Server port")))))
            .Build();

        // Assert
        var json = tool.ToJson();
        var jsonNode = JsonNode.Parse(json)!;

        var parameters = jsonNode["parameters"]!.AsObject();
        var properties = parameters["properties"]!.AsObject();
        var configProp = properties["config"]!.AsObject();

        Assert.Equal("object", configProp["type"]!.GetValue<string>());
        Assert.Equal("Configuration object", configProp["description"]!.GetValue<string>());

        var nestedProps = configProp["properties"]!.AsObject();
        Assert.Contains("host", nestedProps.Select(kv => kv.Key));
        Assert.Contains("port", nestedProps.Select(kv => kv.Key));
    }

    [Fact]
    public void Build_WithNoParameters_CreatesValidToolDefinitionWithoutParameters()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var tool = builder
            .WithName("simple-tool")
            .WithDescription("A tool with no parameters")
            .Build();

        // Assert
        var json = tool.ToJson();
        var jsonNode = JsonNode.Parse(json)!;

        var functionNode = jsonNode.AsObject();
        Assert.False(functionNode.ContainsKey("parameters"));
    }

    [Fact]
    public void WithName_ReturnsBuilderInstance_AllowsMethodChaining()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var result = builder.WithName("test-tool");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void WithDescription_ReturnsBuilderInstance_AllowsMethodChaining()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var result = builder.WithDescription("test description");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void WithParameters_ReturnsBuilderInstance_AllowsMethodChaining()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();

        // Act
        var result = builder.WithParameters(p => { });

        // Assert
        Assert.Same(builder, result);
    }
}

public class ToolDefinitionParameterBuilderTests
{
    [Fact]
    public void Build_WithMultipleProperties_CreatesValidParameterDefinition()
    {
        // Arrange
        var builder = new ToolDefinitionParameterBuilder();

        // Act
        var result = builder
            .WithProperty("prop1", p => p.WithType("string").WithDescription("First property"))
            .WithProperty("prop2", p => p.WithType("integer").WithDescription("Second property"))
            .Build();

        // Assert
        Assert.Equal("object", result["type"]!.GetValue<string>());

        var properties = result["properties"]!.AsObject();
        Assert.Equal(2, properties.Count);
        Assert.Contains("prop1", properties.Select(kv => kv.Key));
        Assert.Contains("prop2", properties.Select(kv => kv.Key));
    }

    [Fact]
    public void Build_WithRequiredProperties_IncludesRequiredArray()
    {
        // Arrange
        var builder = new ToolDefinitionParameterBuilder();

        // Act
        var result = builder
            .WithRequiredProperty("required1", p => p.WithType("string"))
            .WithProperty("optional1", p => p.WithType("integer"))
            .WithRequiredProperty("required2", p => p.WithType("boolean"))
            .Build();

        // Assert
        var required = result["required"]!.AsArray();
        Assert.Equal(2, required.Count);
        Assert.Contains("required1", required.Select(e => e!.GetValue<string>()));
        Assert.Contains("required2", required.Select(e => e!.GetValue<string>()));
    }

    [Fact]
    public void Build_WithNoRequiredProperties_DoesNotIncludeRequiredArray()
    {
        // Arrange
        var builder = new ToolDefinitionParameterBuilder();

        // Act
        var result = builder
            .WithProperty("prop1", p => p.WithType("string"))
            .Build();

        // Assert
        Assert.False(result.ContainsKey("required"));
    }
}

public class ToolDefinitionPropertyBuilderTests
{
    [Fact]
    public void Build_WithStringType_CreatesValidProperty()
    {
        // Arrange
        var builder = new ToolDefinitionPropertyBuilder();

        // Act
        var result = builder
            .WithType(ToolParameterType.String)
            .WithDescription("A string property")
            .Build();

        // Assert
        Assert.Equal("string", result["type"]!.GetValue<string>());
        Assert.Equal("A string property", result["description"]!.GetValue<string>());
    }

    [Fact]
    public void Build_WithEnumValues_IncludesEnumArray()
    {
        // Arrange
        var builder = new ToolDefinitionPropertyBuilder();

        // Act
        var result = builder
            .WithType("string")
            .WithEnum("option1", "option2", "option3")
            .Build();

        // Assert
        var enumArray = result["enum"]!.AsArray();
        Assert.Equal(3, enumArray.Count);
        Assert.Contains("option1", enumArray.Select(e => e!.GetValue<string>()));
        Assert.Contains("option2", enumArray.Select(e => e!.GetValue<string>()));
        Assert.Contains("option3", enumArray.Select(e => e!.GetValue<string>()));
    }

    [Theory]
    [InlineData(ToolParameterType.String, "string")]
    [InlineData(ToolParameterType.StringArray, "array")]
    [InlineData(ToolParameterType.Int, "integer")]
    [InlineData(ToolParameterType.IntArray, "array")]
    [InlineData(ToolParameterType.Decimal, "number")]
    [InlineData(ToolParameterType.DecimalArray, "array")]
    public void WithType_EnumParameter_MapsToCorrectJsonSchemaType(ToolParameterType parameterType, string expectedType)
    {
        // Arrange
        var builder = new ToolDefinitionPropertyBuilder();

        // Act
        var result = builder.WithType(parameterType).Build();

        // Assert
        Assert.Equal(expectedType, result["type"]!.GetValue<string>());
    }

    [Fact]
    public void WithType_InvalidEnumValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var builder = new ToolDefinitionPropertyBuilder();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.WithType((ToolParameterType)999));
    }

    [Fact]
    public void Build_CreatesExpectedOutput()
    {
        // Arrange
        var builder = new ToolDefinitionBuilder();
        var expected = """
{
  "name": "ticks_endpoint",
  "description": "Query trade ticks",
  "parameters": {
    "type": "object",
    "properties": {
      "from": {
        "type": "string",
        "description": "The start date.  Must conform be an RFC3339 date time."
      },
      "until": {
        "type": "string",
        "description": "The end date.  Must conform be an RFC3339 date time."
      }
    },
    "required": [
      "from",
      "until"
    ]
  }
}
""";

        // Act
        var result = builder
            .WithName("ticks_endpoint")
            .WithDescription("Query trade ticks")
            .WithParameters(param => param
                .WithRequiredProperty("from", prop => prop
                    .WithType(ToolParameterType.String)
                    .WithDescription("The start date.  Must conform be an RFC3339 date time."))
                .WithRequiredProperty("until", prop =>
                    prop.WithType(ToolParameterType.String)
                    .WithDescription("The end date.  Must conform be an RFC3339 date time.")))
            .Build();

        // Assert
        Assert.Equal(expected, result.ToJson(pretty: true));
    }
}
