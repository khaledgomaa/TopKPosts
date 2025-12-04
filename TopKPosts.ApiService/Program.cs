using Microsoft.EntityFrameworkCore;
using TopKPosts.ApiService.Consumers;
using TopKPosts.Data;
using TopKPosts.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("postsdb"));
});

builder.EnrichNpgsqlDbContext<AppDbContext>();

builder.Services.AddHostedService<LikesProducer>();
//builder.Services.AddHostedService<PostsConsumer>();
//builder.Services.AddHostedService<LikesConsumer>();
builder.Services.AddHostedService<KafkaRouterConsumer>();

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.AddRedisOutputCache("cache");

builder.AddKafkaProducer<string, string>("kafka");

builder.AddKafkaConsumer<string, string>("kafka", configure =>
{
    configure.Config.GroupId = "consumer-id";
    configure.Config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

app.MapDefaultEndpoints();

app.Run();
