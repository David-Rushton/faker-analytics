using Dr.FakerAnalytics.Cli.Infrastructure;
using Dr.Gemini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console.Cli;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services
    .AddLogging(configure => configure.AddDebug());

services
    .AddOptions<GeminiClientOptions>()
    .BindConfiguration("GeminiClient")
    .ValidateOnStart();

services
    .AddSingleton<IConfiguration>(configuration)
    .AddSingleton<GeminiClient>()
    .AddSingleton<PromptCommand>();

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


    config.AddCommand<DiscoCommand>("disco")
        .WithDescription("Build tools.");
});

await app.RunAsync(args);
