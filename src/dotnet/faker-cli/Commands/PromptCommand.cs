using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dr.GeminiClient;
using Dr.ToolDiscoveryService.Abstractions;
using Markdig;
using Spectre.Console.Cli;

namespace Dr.FakerAnalytics.Cli.Commands;

public class PromptCommand(
    IOptions<GenAiOptions> options,
    ToolExecutor toolExecutor,
    IHttpClientFactory httpClientFactory) : AsyncCommand<PromptCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<prompt>")]
        public required string Prompt { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {

        var conversation = Conversation.Create(options.Value.ApiKey);
        var toolsClient = httpClientFactory.CreateClient("tool-discovery-service");
        var tools = await toolsClient.GetFromJsonAsync<List<Tool>>("/api/tools");
        if (tools is not null && tools.Count != 0)
            conversation.Tools = tools;

        await foreach (var response in conversation.Ask(settings.Prompt))
        {
            switch (response)
            {
                case TextResponse textResponse:
                    AnsiConsole.MarkupLineInterpolated($"[yellow]{textResponse.Text}[/]");
                    break;

                case ThoughtResponse thoughtResponse:
                    AnsiConsole.MarkupLineInterpolated($"[green]{thoughtResponse.Thought}[/]");
                    break;

                case FunctionCallResponse functionCallResponse:
                    AnsiConsole.MarkupLineInterpolated($"[purple]{functionCallResponse.Name} {functionCallResponse.GetJsonArgs()}[/]");

                    var tool = tools!.First(t => t.Name == functionCallResponse.Name);

                    if (tool is null)
                        throw new InvalidOperationException("Unsupported tool requested");

                    var toolResponse = await toolExecutor.ExecuteAsync(
                        tool,
                        functionCallResponse.GetJsonArgs(),
                        CancellationToken.None);

                    // Console.WriteLine(toolResponse);

                    break;

                default:
                    throw new NotSupportedException($"Unknown response type: {nameof(response)}");
            }
        }

        AnsiConsole.WriteLine();

        return 0;
    }
}
