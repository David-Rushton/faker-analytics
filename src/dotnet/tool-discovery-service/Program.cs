var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddFakerAnalyticsServices();

builder.AddServiceDefaults();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.EnableEnvironmentSpecifics();
app.AddDiscoveryEndpoints();

app.Run();
