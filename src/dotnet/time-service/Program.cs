using Dr.TimeService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddNowServices();

builder.AddServiceDefaults();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.EnableEnvironmentSpecifics();
app.AddNowEndpoints();

app.Run();
