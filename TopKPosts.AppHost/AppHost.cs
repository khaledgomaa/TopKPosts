var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres").WithPgAdmin();

var postsDb = postgres.AddDatabase("postsdb");

var kafka = builder.AddKafka("kafka")
                   .WithKafkaUI();

//builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
//{
//    var cs = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct);

//    var config = new AdminClientConfig
//    {
//        BootstrapServers = cs
//    };

//    using var adminClient = new AdminClientBuilder(config).Build();
//    try
//    {
//        await adminClient.CreateTopicsAsync(
//        [
//                new TopicSpecification { Name = "likes-topic", NumPartitions = 2, ReplicationFactor = 1 },
//                new TopicSpecification { Name = "posts-topic", NumPartitions = 1, ReplicationFactor = 1 }
//        ]);
//    }
//    catch (CreateTopicsException e)
//    {
//        Console.WriteLine($"An error occurred creating topic: {e.Message}");
//        throw;
//    }
//});

builder.AddProject<Projects.TopKPosts_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WithReference(postsDb)
    .WaitFor(cache)
    .WaitFor(postsDb);

builder.AddProject<Projects.TopKPosts_Posts_Producer>("topkposts-posts-producer")
    .WithReference(kafka)
    .WaitFor(kafka);

builder.AddProject<Projects.TopKPosts_Likes_Producer>("topkposts-likes-producer")
    .WithReference(kafka)
    .WithReference(postsDb)
    .WaitFor(kafka)
    .WaitFor(postsDb);

builder.AddProject<Projects.TopKPosts_Posts_Consumer>("topkposts-posts-consumer")
    .WithReference(kafka)
    .WithReference(postsDb)
    .WaitFor(kafka)
    .WaitFor(postsDb);

builder.AddProject<Projects.TopKPosts_Likes_Consumer>("topkposts-likes-consumer")
    .WithReference(kafka)
    .WithReference(postsDb)
    .WaitFor(kafka)
    .WaitFor(postsDb);

builder.AddProject<Projects.TopKPosts_Ranking>("topkposts-ranking")
    .WithReference(kafka)
    .WithReference(cache)
    .WaitFor(kafka)
    .WaitFor(cache);

builder.Build().Run();
