# Faker Analytics

I built this to explore:

- Integrating generative AI into traditional applications
- Providing AI with tools to augment its capabilities  
- Extending existing apps with AI features

The "traditional application" is `faker-analytics` - a collection of APIs providing energy trading 
market insights. All data is fake (hence the name). The APIs cover basic functionality since building
a true analytics system isn't the goal here.

I'm keeping a [blog](./blog/), so you can track my progress.

## Getting Started

1. Check the [prerequisites](#prerequisites)

```pwsh
./scripts/Test-Prerequisites.ps1
```

2. Start the services

Using [Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview), an 
orchestration system designed to simplify building and deploying distributed applications, especially
for local environments.

```pwsh
# Output includes a dashboard link for viewing logs, metrics, and traces
dotnet run ./src/dotnet/faker-apphost/ --profile http
```

> **PRO TIP**: The dashboard supports Copilot!  
> Log into [GitHub Copilot] via Visual Studio or VS Code, then start the app host project from the editor.

3. Chat with the CLI

```pwsh
./scripts/fa prompt "what can you do?"

# Perhaps you have more to say?
./scripts/fa prompt "

I would like to see all trades from Nov last year.  For Dutch natural gas.  With delivery in the next
month.

"
```

### Prerequisites

Apologies.  This is a longer list than I would like.  But this repo is an experiment.  Generating 
something that is easy to consume isn't really the point. 

#### Gemini API Key

You'll need a Gemini API key from Google AI Studio or Google Cloud.
Get one [here](https://ai.google.dev/gemini-api/docs/api-key).

#### Docker 

You'll need [Docker](https://docs.docker.com/engine/install/).

Untested with Podman or Rancher, but they should work.

#### .Net

You'll need [.NET](https://dotnet.microsoft.com/en-us/download) `10.0.100-RC1` or higher.

In theory .Net 8.0 or higher should work.  But I haven't tested it.

## PowerShell 

You'll need [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.5).

Any recent version works. Really old versions __probably__ work.

<!-- links -->

[GitHub Copilot]: https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/copilot
