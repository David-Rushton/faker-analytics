using Dr.GeminiClient;
using Markdig;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Dr.FakerAnalytics.Cli.Commands;

public class PromptCommand(GeminiClient.GeminiClient geminiClient) : AsyncCommand<PromptCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<prompt>")]
        public required string Prompt { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
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
}
