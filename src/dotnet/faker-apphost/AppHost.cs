var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.faker_api>("faker-api")
    .WithExternalHttpEndpoints();

builder.Build().Run();
