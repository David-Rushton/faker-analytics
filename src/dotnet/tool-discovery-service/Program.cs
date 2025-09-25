var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddOpenApi()
    .AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.EnableEnvironmentSpecifics();
app.AddDiscoveryEndpoints();

app.Run();
