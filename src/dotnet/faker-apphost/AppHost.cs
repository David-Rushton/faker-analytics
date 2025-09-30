var builder = DistributedApplication.CreateBuilder(args);

var toolDiscoveryService = builder.AddProject<Projects.tool_discovery_service>("tool-discovery-service")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

var fakerApi = builder.AddProject<Projects.faker_api>("faker-api")
    .WithExternalHttpEndpoints()
    .WithReference(toolDiscoveryService)
    .WithHttpHealthCheck("/health");

var timeService = builder.AddProject<Projects.time_service>("time-service")
    .WithExternalHttpEndpoints()
    .WithReference(toolDiscoveryService)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
