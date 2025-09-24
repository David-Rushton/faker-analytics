using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dr.Gemini;

public class JsonStringOrArrayConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString()!;
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Read the first item in the array
            if (reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                var firstItem = reader.GetString();
                // Skip the rest of the array to avoid a JSON parsing error
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    // Do nothing, just consume the remaining tokens
                }
                return firstItem!;
            }
            else
            {
                throw new JsonException("Expected array with at least one string.");
            }
        }

        throw new JsonException("Expected a string or an array of strings.");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
