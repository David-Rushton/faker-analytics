using System.Net.Http.Json;
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

        conversation.SystemInstruction = """
You are an agent.  Your purpose is to help the user investigate the energy commodities market.  Using
the tools provided.

When constructing function calls you **must** format any parameters called `from` or `until` in strict
RFC3339 format.  Do not include any spaces or non standard ASCII characters.  There is no need to
provide anything below a second granularity.
""";

        AnsiConsole.MarkupLineInterpolated($"[purple]Available tools: {string.Join(", ", tools!.Select(t => t.Name))}.[/]");

        try
        {
            await HandleResponses(conversation.Ask(settings.Prompt));
        }
        catch (HttpRequestException e)
        {
            AnsiConsole.WriteException(e);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }

        return 0;

        async Task HandleResponses(IAsyncEnumerable<Response> responses)
        {
            await foreach (var response in responses)
            {
                switch (response)
                {
                    case TextResponse textResponse:
                        AnsiConsole.MarkupInterpolated($"[yellow]{textResponse.Text}[/]");
                        break;

                    case ThoughtResponse thoughtResponse:
                        AnsiConsole.MarkupLineInterpolated($"[green]{thoughtResponse.Thought}[/]");
                        break;

                    case FunctionCallResponse functionCallResponse:
                        // AnsiConsole.Markup("[purple]");
                        AnsiConsole.WriteLine($"{functionCallResponse.Name} {functionCallResponse.GetJsonArgs()}");
                        // AnsiConsole.Markup("[/]");

                        var tool = tools!.First(t => t.Name == functionCallResponse.Name);

                        if (tool is null)
                            throw new InvalidOperationException("Unsupported tool requested");

                        var toolResponse = await toolExecutor.ExecuteAsync(
                            tool,
                            functionCallResponse.GetJsonArgs(),
                            CancellationToken.None);

                        await HandleResponses(conversation.ReplyWithFunctionResult(tool, toolResponse));

                        break;

                    default:
                        throw new NotSupportedException($"Unknown response type: {nameof(response)}");
                }
            }

            AnsiConsole.WriteLine();
        }
    }
}
