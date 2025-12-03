using Microsoft.EntityFrameworkCore;
using TopKPosts.ApiService.Services;
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

builder.Services.AddHostedService<PostsWorker>();
builder.Services.AddHostedService<LikesWorker>();

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.AddRedisOutputCache("cache");

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
