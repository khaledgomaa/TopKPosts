var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres").WithPgAdmin();

var postsDb = postgres.AddDatabase("postsdb");

var apiService = builder.AddProject<Projects.TopKPosts_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(postsDb)
    .WithReference(cache);

builder.AddProject<Projects.TopKPosts_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
