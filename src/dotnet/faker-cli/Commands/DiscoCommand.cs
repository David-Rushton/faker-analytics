using System.Text;
using Dr.Gemini;
using Markdig;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Dr.FakerAnalytics.Cli.Commands;

public class DiscoCommand(GeminiClient geminiClient) : AsyncCommand<DiscoCommand.Settings>
{
    private const string OpenApiUrl = "http://localhost:5250/openapi/v1.json";
    private const string OpenApiJson = """
{"openapi":"3.1.1","info":{"title":"faker-api | v1","version":"1.0.0"},"servers":[{"url":"http://localhost:5250/"}],"paths":{"/api/trades":{"get":{"tags":["faker-api"],"description":"    Returns trades.\n\n    1, and only 1, instrumentId and commingleId must be supplied.\n\n    from must be before until.","operationId":"Trades","parameters":[{"name":"from","in":"query","required":true,"schema":{"type":"string","format":"date-time"}},{"name":"until","in":"query","required":true,"schema":{"type":"string","format":"date-time"}},{"name":"instrumentId","in":"query","schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"commingleId","in":"query","schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"sequenceId","in":"query","required":true,"schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"sequenceItemId","in":"query","required":true,"schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"contractType","in":"query","required":true,"schema":{"$ref":"#/components/schemas/ContractType"}}],"responses":{"200":{"description":"OK"}}}},"/api/trades/ohlc":{"get":{"tags":["faker-api"],"description":"    Returns open high low close (OHLC) candles.\n\n    1, and only 1, instrumentId and commingleId must be supplied.\n\n    from must be before until.","operationId":"Trades OHLC","parameters":[{"name":"from","in":"query","required":true,"schema":{"type":"string","format":"date-time"}},{"name":"until","in":"query","required":true,"schema":{"type":"string","format":"date-time"}},{"name":"instrumentId","in":"query","schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"commingleId","in":"query","schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"sequenceId","in":"query","required":true,"schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"sequenceItemId","in":"query","required":true,"schema":{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}},{"name":"contractType","in":"query","required":true,"schema":{"$ref":"#/components/schemas/ContractType"}}],"responses":{"200":{"description":"OK"}}}}},"components":{"schemas":{"ContractType":{"type":"integer"}}},"tags":[{"name":"faker-api"}]}
""";
    private const string promptTemplate = """
You are an **OpenAPI to Gemini Function Calling translator**. Your sole purpose is to convert a
given OpenAPI 3.0 JSON specification into the tools array format for the Google Gemini API's
function calling feature. Do not engage in any conversation.

The output **must** be a valid JSON array containing one or more tool objects, as per the Gemini
API's tool definition schema. Each object must contain a `name`, `description`, and a `parameters` property
conforming to JSON Schema. If a valid name cannot be inferred or if the translation is not possible,
reply with a JSON object following the `application/problem+json` standard.

The OpenAPI spec is:

```
<OpenApiSpec>
```
""";

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<prompt>")]
        public required string Prompt { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var prompt = promptTemplate.Replace("<OpenApiSpec>", OpenApiJson);
        var tools = new StringBuilder();
        Console.WriteLine(prompt);

        await foreach (var (isThought, part, functionCall) in geminiClient.GetResponseStream(prompt))
        {
            if (isThought)
            {
                AnsiConsole.Markup($"[grey]{part.EscapeMarkup()}[/]");
            }

            if (functionCall is not null)
            {
                AnsiConsole.Markup($"[green]{functionCall.ToString().EscapeMarkup()}[/]");
                continue;
            }

            if (string.IsNullOrEmpty(part))
            {
                AnsiConsole.Markup($"{part.EscapeMarkup()}");
            }
        }

        AnsiConsole.WriteLine();

        // ---------------------------

        Console.WriteLine(stash);
        geminiClient.AddTools(stash);

        await foreach (var (isThought, part, functionCall) in geminiClient.GetResponseStream(settings.Prompt))
        {
            if (isThought)
            {
                AnsiConsole.Markup($"[grey]{part.EscapeMarkup()}[/]");
            }

            if (functionCall is not null)
            {
                AnsiConsole.Markup($"[green]{functionCall.ToString().EscapeMarkup()}[/]");
                continue;
            }

            if (string.IsNullOrEmpty(part))
            {
                AnsiConsole.Markup($"{part.EscapeMarkup()}");
            }
        }

        AnsiConsole.WriteLine();


        return 0;
    }


    private string stash => """
[
  {
    "name": "Trades",
    "description": "Returns trades. 1, and only 1, instrumentId and commingleId must be supplied. from must be before until.",
    "parameters": {
      "type": "object",
      "properties": {
        "from": {
          "type": "string",
          "format": "date-time"
        },
        "until": {
          "type": "string",
          "format": "date-time"
        },
        "instrumentId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "commingleId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "sequenceId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "sequenceItemId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "contractType": {
          "type": "integer"
        }
      },
      "required": [
        "from",
        "until",
        "sequenceId",
        "sequenceItemId",
        "contractType"
      ]
    }
  },
  {
    "name": "Trades_OHLC",
    "description": "Returns open high low close (OHLC) candles. 1, and only 1, instrumentId and commingleId must be supplied. from must be before until.",
    "parameters": {
      "type": "object",
      "properties": {
        "from": {
          "type": "string",
          "format": "date-time"
        },
        "until": {
          "type": "string",
          "format": "date-time"
        },
        "instrumentId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "commingleId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "sequenceId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "sequenceItemId": {
          "type": [
            "integer",
            "string"
          ],
          "format": "int32",
          "pattern": "^-?(?:0|[1-9]\\d*)$"
        },
        "contractType": {
          "type": "integer"
        }
      },
      "required": [
        "from",
        "until",
        "sequenceId",
        "sequenceItemId",
        "contractType"
      ]
    }
  }
]
""";
}
