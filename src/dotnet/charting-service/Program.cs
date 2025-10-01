var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddChartingServices()
    .AddOpenApi()
    .AddEndpointsApiExplorer();

builder.AddServiceDefaults();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.EnableEnvironmentSpecifics();
app.AddChartingEndpoints();

app.Run();
