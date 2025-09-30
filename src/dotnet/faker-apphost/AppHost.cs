using System.Reflection;

var builder = DistributedApplication.CreateBuilder(args);

var toolDiscoveryService = builder.AddProject<Projects.tool_discovery_service>("tool-discovery-service", launchProfileName: "http")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

var fakerApi = builder.AddProject<Projects.faker_api>("faker-api", launchProfileName: "http")
    .WithExternalHttpEndpoints()
    .WithReference(toolDiscoveryService)
    .WithHttpHealthCheck("/health");

var fakerMeta = builder.AddProject<Projects.faker_meta>("faker-meta", launchProfileName: "http")
    .WithExternalHttpEndpoints()
    .WithReference(toolDiscoveryService)
    .WithHttpHealthCheck("/health");

var timeService = builder.AddProject<Projects.time_service>("time-service", launchProfileName: "http")
    .WithExternalHttpEndpoints()
    .WithReference(toolDiscoveryService)
    .WithHttpHealthCheck("/health");


builder.Build().Run();
