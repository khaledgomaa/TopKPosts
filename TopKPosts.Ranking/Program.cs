using TopKPosts.Ranking;
using TopKPosts.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddRedisOutputCache("cache");

builder.AddKafkaConsumer<string, string>("kafka", configure =>
{
    configure.Config.GroupId = "ranking-consumer-group-id";
    configure.Config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
    configure.Config.EnableAutoCommit = true;
});

builder.Services.AddHostedService<RankingWorker>();

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
