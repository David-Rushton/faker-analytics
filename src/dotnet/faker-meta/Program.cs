using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddFakerMetaServices();

builder.AddServiceDefaults();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.EnableEnvironmentSpecifics();
app.AddFakerMetaEndpoints();

app.Run();
