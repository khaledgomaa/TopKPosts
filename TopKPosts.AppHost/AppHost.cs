var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres").WithPgAdmin();

var postsDb = postgres.AddDatabase("postsdb");

var apiService = builder.AddProject<Projects.TopKPosts_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(postsDb)
    .WithReference(cache)
    .WaitFor(postsDb)
    .WaitFor(cache);

builder.AddProject<Projects.TopKPosts_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WithReference(postsDb)
    .WithReference(apiService)
    .WaitFor(cache)
    .WaitFor(postsDb)
    .WaitFor(apiService)
    .WaitFor(apiService);

builder.Build().Run();
