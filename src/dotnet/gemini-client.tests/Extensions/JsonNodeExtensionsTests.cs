using Dr.GeminiClient.Extensions;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Dr.GeminiClient.Tests.Extensions;

public class JsonNodeExtensionsTests
{
    [Fact]
    public void ToProtobufStruct_WithSimpleObject_ReturnsCorrectStruct()
    {
        // Arrange
        var json = """
        {
            "name": "John",
            "age": 30,
            "active": true
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Equal(3, result.Fields.Count);
        Assert.Equal("John", result.Fields["name"].StringValue);
        Assert.Equal(30, result.Fields["age"].NumberValue);
        Assert.True(result.Fields["active"].BoolValue);
    }

    [Fact]
    public void ToProtobufStruct_WithNestedObject_ReturnsCorrectStruct()
    {
        // Arrange
        var json = """
        {
            "user": {
                "name": "Jane",
                "id": 123
            },
            "tags": ["admin", "user"]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Equal(2, result.Fields.Count);

        // Check nested object
        var userStruct = result.Fields["user"].StructValue;
        Assert.Equal("Jane", userStruct.Fields["name"].StringValue);
        Assert.Equal(123, userStruct.Fields["id"].NumberValue);

        // Check array
        var tagsArray = result.Fields["tags"].ListValue;
        Assert.Equal(2, tagsArray.Values.Count);
        Assert.Equal("admin", tagsArray.Values[0].StringValue);
        Assert.Equal("user", tagsArray.Values[1].StringValue);
    }

    [Fact]
    public void ToProtobufValue_WithString_ReturnsStringValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create("test string");

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal("test string", result.StringValue);
    }

    [Fact]
    public void ToProtobufValue_WithNumber_ReturnsNumberValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(42.5);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal(42.5, result.NumberValue);
    }

    [Fact]
    public void ToProtobufValue_WithInteger_ReturnsNumberValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(42);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal(42, result.NumberValue);
    }

    [Fact]
    public void ToProtobufValue_WithLong_ReturnsNumberValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(9223372036854775807L);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal(9223372036854775807L, result.NumberValue);
    }

    [Fact]
    public void ToProtobufValue_WithFloat_ReturnsNumberValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(3.14f);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal(3.14f, result.NumberValue, 6);
    }

    [Fact]
    public void ToProtobufValue_WithDecimal_ReturnsNumberValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(123.45m);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal(123.45, result.NumberValue, 6);
    }

    [Fact]
    public void ToProtobufValue_WithBoolean_ReturnsBoolValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(true);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.True(result.BoolValue);
    }

    [Fact]
    public void ToProtobufValue_WithFalse_ReturnsBoolValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create(false);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.False(result.BoolValue);
    }

    [Fact]
    public void ToProtobufValue_WithNull_ReturnsNullValue()
    {
        // Arrange
        var jsonNode = JsonValue.Create((string?)null);

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        Assert.Equal(NullValue.NullValue, result.NullValue);
    }

    [Fact]
    public void ToProtobufValue_WithArray_ReturnsListValue()
    {
        // Arrange
        var json = """["apple", "banana", "cherry"]""";
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        var listValue = result.ListValue;
        Assert.Equal(3, listValue.Values.Count);
        Assert.Equal("apple", listValue.Values[0].StringValue);
        Assert.Equal("banana", listValue.Values[1].StringValue);
        Assert.Equal("cherry", listValue.Values[2].StringValue);
    }

    [Fact]
    public void ToProtobufValue_WithMixedArray_ReturnsListValue()
    {
        // Arrange
        var json = """[123, "text", true, null]""";
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        var listValue = result.ListValue;
        Assert.Equal(4, listValue.Values.Count);
        Assert.Equal(123, listValue.Values[0].NumberValue);
        Assert.Equal("text", listValue.Values[1].StringValue);
        Assert.True(listValue.Values[2].BoolValue);
        Assert.Equal(NullValue.NullValue, listValue.Values[3].NullValue);
    }

    [Fact]
    public void ToProtobufStruct_WithNonObjectJsonNode_ThrowsArgumentException()
    {
        // Arrange
        var jsonNode = JsonValue.Create("not an object");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => jsonNode.ToProtobufStruct());
        Assert.Contains("JsonNode must be a JsonObject", exception.Message);
    }

    [Fact]
    public void ToProtobufStruct_WithComplexTradeData_ReturnsCorrectStruct()
    {
        // Arrange - simulate trade data that might come from your API
        var json = """
        {
            "trades": [
                {
                    "id": "trade-001",
                    "symbol": "AAPL",
                    "price": 150.25,
                    "quantity": 100,
                    "timestamp": "2024-01-15T10:30:00Z",
                    "metadata": {
                        "exchange": "NASDAQ",
                        "type": "limit"
                    }
                }
            ],
            "total": 1,
            "success": true
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Equal(3, result.Fields.Count);
        Assert.Equal(1, result.Fields["total"].NumberValue);
        Assert.True(result.Fields["success"].BoolValue);

        var tradesArray = result.Fields["trades"].ListValue;
        Assert.Single(tradesArray.Values);

        var trade = tradesArray.Values[0].StructValue;
        Assert.Equal("trade-001", trade.Fields["id"].StringValue);
        Assert.Equal("AAPL", trade.Fields["symbol"].StringValue);
        Assert.Equal(150.25, trade.Fields["price"].NumberValue);
        Assert.Equal(100, trade.Fields["quantity"].NumberValue);

        var metadata = trade.Fields["metadata"].StructValue;
        Assert.Equal("NASDAQ", metadata.Fields["exchange"].StringValue);
        Assert.Equal("limit", metadata.Fields["type"].StringValue);
    }

    [Fact]
    public void ToProtobufStruct_WithEmptyObject_ReturnsEmptyStruct()
    {
        // Arrange
        var json = """{}""";
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Empty(result.Fields);
    }

    [Fact]
    public void ToProtobufValue_WithEmptyArray_ReturnsEmptyListValue()
    {
        // Arrange
        var json = """[]""";
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufValue();

        // Assert
        var listValue = result.ListValue;
        Assert.Empty(listValue.Values);
    }

    [Fact]
    public void ToProtobufStruct_WithNestedArraysAndObjects_ReturnsCorrectStruct()
    {
        // Arrange
        var json = """
        {
            "data": {
                "users": [
                    {"name": "Alice", "roles": ["admin", "user"]},
                    {"name": "Bob", "roles": ["user"]}
                ],
                "count": 2
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        var dataStruct = result.Fields["data"].StructValue;
        Assert.Equal(2, dataStruct.Fields["count"].NumberValue);

        var usersArray = dataStruct.Fields["users"].ListValue;
        Assert.Equal(2, usersArray.Values.Count);

        var alice = usersArray.Values[0].StructValue;
        Assert.Equal("Alice", alice.Fields["name"].StringValue);
        var aliceRoles = alice.Fields["roles"].ListValue;
        Assert.Equal(2, aliceRoles.Values.Count);
        Assert.Equal("admin", aliceRoles.Values[0].StringValue);
        Assert.Equal("user", aliceRoles.Values[1].StringValue);

        var bob = usersArray.Values[1].StructValue;
        Assert.Equal("Bob", bob.Fields["name"].StringValue);
        var bobRoles = bob.Fields["roles"].ListValue;
        Assert.Single(bobRoles.Values);
        Assert.Equal("user", bobRoles.Values[0].StringValue);
    }

    [Fact]
    public void ToProtobufStruct_WithJsonArray_WrapsInDataField()
    {
        // Arrange
        var json = """["apple", "banana", "cherry"]""";
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Single(result.Fields);
        Assert.True(result.Fields.ContainsKey("data"));

        var dataArray = result.Fields["data"].ListValue;
        Assert.Equal(3, dataArray.Values.Count);
        Assert.Equal("apple", dataArray.Values[0].StringValue);
        Assert.Equal("banana", dataArray.Values[1].StringValue);
        Assert.Equal("cherry", dataArray.Values[2].StringValue);
    }

    [Fact]
    public void ToProtobufStruct_WithJsonArrayDirectly_WrapsInDataField()
    {
        // Arrange
        var json = """[1, 2, 3]""";
        var jsonArray = JsonNode.Parse(json)!.AsArray();

        // Act
        var result = jsonArray.ToProtobufStruct();

        // Assert
        Assert.Single(result.Fields);
        Assert.True(result.Fields.ContainsKey("data"));

        var dataArray = result.Fields["data"].ListValue;
        Assert.Equal(3, dataArray.Values.Count);
        Assert.Equal(1, dataArray.Values[0].NumberValue);
        Assert.Equal(2, dataArray.Values[1].NumberValue);
        Assert.Equal(3, dataArray.Values[2].NumberValue);
    }

    [Fact]
    public void ToProtobufStruct_WithJsonObjectDirectly_ReturnsCorrectStruct()
    {
        // Arrange
        var json = """{"name": "test", "value": 42}""";
        var jsonObject = JsonNode.Parse(json)!.AsObject();

        // Act
        var result = jsonObject.ToProtobufStruct();

        // Assert
        Assert.Equal(2, result.Fields.Count);
        Assert.Equal("test", result.Fields["name"].StringValue);
        Assert.Equal(42, result.Fields["value"].NumberValue);
    }

    [Fact]
    public void ToProtobufStruct_WithComplexJsonArray_WrapsCorrectly()
    {
        // Arrange
        var json = """
        [
            {"id": 1, "name": "Alice"},
            {"id": 2, "name": "Bob"}
        ]
        """;
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Single(result.Fields);

        var dataArray = result.Fields["data"].ListValue;
        Assert.Equal(2, dataArray.Values.Count);

        var alice = dataArray.Values[0].StructValue;
        Assert.Equal(1, alice.Fields["id"].NumberValue);
        Assert.Equal("Alice", alice.Fields["name"].StringValue);

        var bob = dataArray.Values[1].StructValue;
        Assert.Equal(2, bob.Fields["id"].NumberValue);
        Assert.Equal("Bob", bob.Fields["name"].StringValue);
    }

    [Fact]
    public void ToProtobufStruct_WithNullJsonNode_ThrowsArgumentException()
    {
        // Arrange
        JsonNode? jsonNode = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => jsonNode!.ToProtobufStruct());
        Assert.Contains("JsonNode must be a JsonObject or JsonArray", exception.Message);
        Assert.Contains("null", exception.Message);
    }

    [Fact]
    public void ToProtobufStruct_WithJsonValue_ThrowsArgumentException()
    {
        // Arrange
        var jsonNode = JsonValue.Create("simple string");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => jsonNode.ToProtobufStruct());
        Assert.Contains("JsonNode must be a JsonObject or JsonArray", exception.Message);
        Assert.Contains("JsonValue", exception.Message);
    }

    [Fact]
    public void ToProtobufStruct_WithEmptyJsonArray_ReturnsStructWithEmptyDataArray()
    {
        // Arrange
        var json = """[]""";
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Single(result.Fields);
        Assert.True(result.Fields.ContainsKey("data"));

        var dataArray = result.Fields["data"].ListValue;
        Assert.Empty(dataArray.Values);
    }

    [Fact]
    public void ToProtobufStruct_WithTradeDataAsArray_WrapsCorrectly()
    {
        // Arrange - simulate API response that returns array of trades
        var json = """
        [
            {
                "id": "trade-001",
                "symbol": "AAPL",
                "price": 150.25,
                "quantity": 100
            },
            {
                "id": "trade-002",
                "symbol": "GOOGL",
                "price": 2800.50,
                "quantity": 50
            }
        ]
        """;
        var jsonNode = JsonNode.Parse(json)!;

        // Act
        var result = jsonNode.ToProtobufStruct();

        // Assert
        Assert.Single(result.Fields);

        var tradesArray = result.Fields["data"].ListValue;
        Assert.Equal(2, tradesArray.Values.Count);

        var trade1 = tradesArray.Values[0].StructValue;
        Assert.Equal("trade-001", trade1.Fields["id"].StringValue);
        Assert.Equal("AAPL", trade1.Fields["symbol"].StringValue);
        Assert.Equal(150.25, trade1.Fields["price"].NumberValue);
        Assert.Equal(100, trade1.Fields["quantity"].NumberValue);

        var trade2 = tradesArray.Values[1].StructValue;
        Assert.Equal("trade-002", trade2.Fields["id"].StringValue);
        Assert.Equal("GOOGL", trade2.Fields["symbol"].StringValue);
        Assert.Equal(2800.50, trade2.Fields["price"].NumberValue);
        Assert.Equal(50, trade2.Fields["quantity"].NumberValue);
    }
}
