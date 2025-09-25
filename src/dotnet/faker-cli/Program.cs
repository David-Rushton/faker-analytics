using Dr.FakerAnalytics.Cli.Infrastructure;
using Dr.GeminiClient.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services
    .AddGeminiClient()
    .AddSingleton<IConfiguration>(configuration)
    .AddSingleton<PromptCommand>()
    .AddLogging(configure => configure.AddDebug());

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.SetApplicationName("faker-cli");
    config.SetApplicationVersion("0.1.0");
    config.CaseSensitivity(CaseSensitivity.None);
    config.PropagateExceptions();

    config.AddCommand<PromptCommand>("prompt")
        .WithDescription("Get a response from the Gemini model for a given prompt.");
});

await app.RunAsync(args);
