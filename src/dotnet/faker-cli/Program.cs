using Dr.FakerAnalytics.Cli.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services
    .AddSingleton<IConfiguration>(configuration)
    .AddSingleton<PromptCommand>()
    .AddSingleton<ToolExecutor>()
    .AddLogging(configure => configure.AddDebug())
    .AddHttpClient("tool-discovery-service", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5065");
    });

services
    .AddOptions<GenAiOptions>()
    .BindConfiguration(GenAiOptions.Name)
    .ValidateOnStart();

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
