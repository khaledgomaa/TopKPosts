using Microsoft.EntityFrameworkCore;
using TopKPosts.Data;
using TopKPosts.Posts.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHostedService<PostsConsumer>();

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("postsdb"));
});

builder.EnrichNpgsqlDbContext<AppDbContext>();

builder.AddKafkaConsumer<string, string>("kafka", configure =>
{
    configure.Config.GroupId = "posts-consumer-group-id";
    configure.Config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
});

var app = builder.Build();

var scope = app.Services.CreateScope();
var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
using var dbContext = dbContextFactory.CreateDbContext();
dbContext.Database.Migrate();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
