using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json;

namespace Dr.GeminiClient.Extensions;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Converts a JsonNode to a Google Protobuf Struct
    /// </summary>
    public static Struct ToProtobufStruct(this JsonNode jsonNode)
    {
        return jsonNode switch
        {
            JsonObject jsonObject => ConvertJsonObjectToStruct(jsonObject),
            JsonArray jsonArray => ConvertJsonArrayToStruct(jsonArray),
            _ => throw new ArgumentException($"JsonNode must be a JsonObject or JsonArray to convert to Struct. Received: {jsonNode?.GetType()?.Name ?? "null"}", nameof(jsonNode))
        };
    }

    /// <summary>
    /// Converts a JsonObject directly to a Google Protobuf Struct
    /// </summary>
    public static Struct ToProtobufStruct(this JsonObject jsonObject)
    {
        return ConvertJsonObjectToStruct(jsonObject);
    }

    /// <summary>
    /// Converts a JsonArray to a Google Protobuf Struct by wrapping it in a "data" field
    /// </summary>
    public static Struct ToProtobufStruct(this JsonArray jsonArray)
    {
        return ConvertJsonArrayToStruct(jsonArray);
    }

    /// <summary>
    /// Converts a JsonNode to a Google Protobuf Value
    /// </summary>
    public static Value ToProtobufValue(this JsonNode? jsonNode)
    {
        if (jsonNode == null)
        {
            return new Value { NullValue = NullValue.NullValue };
        }

        return jsonNode switch
        {
            JsonValue jsonValue when jsonValue.TryGetValue<string>(out var stringValue) =>
                new Value { StringValue = stringValue },
            JsonValue jsonValue when jsonValue.TryGetValue<bool>(out var boolValue) =>
                new Value { BoolValue = boolValue },
            JsonValue jsonValue when jsonValue.TryGetValue<double>(out var doubleValue) =>
                new Value { NumberValue = doubleValue },
            JsonValue jsonValue when jsonValue.TryGetValue<int>(out var intValue) =>
                new Value { NumberValue = intValue },
            JsonValue jsonValue when jsonValue.TryGetValue<long>(out var longValue) =>
                new Value { NumberValue = longValue },
            JsonValue jsonValue when jsonValue.TryGetValue<float>(out var floatValue) =>
                new Value { NumberValue = floatValue },
            JsonValue jsonValue when jsonValue.TryGetValue<decimal>(out var decimalValue) =>
                new Value { NumberValue = (double)decimalValue },
            JsonObject jsonObject => ConvertJsonObjectToValue(jsonObject),
            JsonArray jsonArray => ConvertJsonArrayToValue(jsonArray),
            _ when jsonNode.GetValueKind() == JsonValueKind.Null =>
                new Value { NullValue = NullValue.NullValue },
            _ => throw new ArgumentException($"Unsupported JsonNode type: {jsonNode.GetType()}")
        };
    }

    private static Struct ConvertJsonObjectToStruct(JsonObject jsonObject)
    {
        var structValue = new Struct();
        foreach (var kvp in jsonObject)
        {
            // Handle null values in the struct
            structValue.Fields[kvp.Key] = kvp.Value.ToProtobufValue();
        }
        return structValue;
    }

    private static Struct ConvertJsonArrayToStruct(JsonArray jsonArray)
    {
        // Wrap the array in a struct with a "data" field
        var structValue = new Struct();
        structValue.Fields["data"] = ConvertJsonArrayToValue(jsonArray);
        return structValue;
    }

    private static Value ConvertJsonObjectToValue(JsonObject jsonObject)
    {
        var structValue = new Struct();
        foreach (var kvp in jsonObject)
        {
            // Handle null values in object properties
            structValue.Fields[kvp.Key] = kvp.Value.ToProtobufValue();
        }
        return new Value { StructValue = structValue };
    }

    private static Value ConvertJsonArrayToValue(JsonArray jsonArray)
    {
        var listValue = new ListValue();
        foreach (var item in jsonArray)
        {
            // Include null items in the array - convert them to protobuf null values
            listValue.Values.Add(item.ToProtobufValue());
        }
        return new Value { ListValue = listValue };
    }
}
