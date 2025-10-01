using System.Text;
using Dr.GeminiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dr.ChartingService.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication EnableEnvironmentSpecifics(this WebApplication app)
    {
        var logger = app.Logger;
        logger.LogInformation("Charting Service starting up...");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            logger.LogInformation("OpenAPI documentation enabled for development");
        }

        return app;
    }

    public static WebApplication AddChartingEndpoints(this WebApplication app)
    {
        app.AddPostWebpage();
        app.AddGetWebpage();

        return app;
    }

    private static WebApplication AddPostWebpage(this WebApplication app)
    {
        app.MapPost("/api/webpages", async (
            [FromServices] ILogger<Program> logger,
            [FromServices] WebpageRepository repository,
            [FromHeader(Name = "X-Goog-Api-Key")] string apiKey,
            [FromBody] WebpageRequest requestParameters
        ) =>
        {
            logger.LogInformation("POST /api/webpages");

            var conversation = GeminiClient.Conversation.Create(apiKey);
            conversation.SystemInstruction = """
You are an agent specializing in front-end development. When given a prompt that includes a dataset,
your task is to create a single, complete HTML page. This page must be self-contained, with all
necessary JavaScript and CSS embedded directly within <script> and <style> tags, respectively.

The final HTML page must:

Plot the dataset: Visualize the provided data using an appropriate chart type. If no specific chart
type is mentioned, select the most suitable one based on the data.

Display the dataset: Present the raw data in a clear, well-formatted HTML <table>.

Be self-contained: Do not reference external files for CSS or JavaScript, except for common libraries
that can be imported via a <script src="..."> tag.

Exclude Python: The final response must not contain any Python code.

Your response must only be the final HTML page. If you are unable to complete the request for any
reason, respond with 404 and nothing else.
""";
            var webpageId = Guid.NewGuid();
            var webpage = new StringBuilder();

            logger.LogInformation("Creating webpage: {webpageId}", webpageId);

            await foreach (var response in conversation.Ask(requestParameters.Prompt))
            {
                switch (response)
                {
                    case ThoughtResponse thoughtResponse:
                        logger.LogInformation("Thinking during webpage {webpageId} creation: {thought}", webpageId, thoughtResponse.Thought);
                        break;

                    case TextResponse textResponse:
                        if (textResponse.Text == "404")
                            return Results.InternalServerError("The model was unable to process the request");

                        logger.LogInformation("Adding to webpage: {content}", textResponse.Text);
                        webpage.Append(textResponse.Text);


                        break;

                    default:
                        logger.LogError("Unexpected reply while building webpage {webpageId}", webpageId);
                        return Results.InternalServerError("Unsupported gen AI request type");
                }
            }

            repository.Add(webpageId, webpage.ToString());

            return Results.Ok(new WebpageResponse { WebpageId = webpageId });
        })
        .WithName("create_webpage")
        .WithDescription("Generates a webpage using the provided prompt.")
        .Produces<WebpageResponse>();

        return app;
    }

    private static WebApplication AddGetWebpage(this WebApplication app)
    {
        app.MapGet("/api/webpages/{webpageId}", (
            Guid webpageId,
            [FromServices] ILogger<Program> logger,
            [FromServices] WebpageRepository repository
        ) =>
        {
            logger.LogInformation("GET api/webpage/{webpageId}", webpageId);

            if (repository.TryGet(webpageId, out var webpage))
                return Results.Content(webpage);

            return Results.NotFound();
        })
        .WithName("get_webpage")
        .WithDescription("""
Returns a webpage.

Webpages generated via POST /api/webpage are persisted and can be recalled later.
""");

        return app;
    }
}
