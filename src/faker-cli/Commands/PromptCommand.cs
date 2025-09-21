using Dr.Gemini;
using Markdig;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Dr.FakerAnalytics.Cli.Commands;

public class PromptCommand(GeminiClient geminiClient) : AsyncCommand<PromptCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<prompt>")]
        public required string Prompt { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await foreach (var (isThought, part) in geminiClient.GetResponseStream(settings.Prompt))
        {
            if (isThought)
            {
                AnsiConsole.MarkupLine($"[grey]{part.EscapeMarkup()}[/]");
                continue;
            }


            AnsiConsole.MarkupLine($"{part.EscapeMarkup()}");
        }

        return 0;
    }
}
