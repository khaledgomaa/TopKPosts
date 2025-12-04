using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TopKPosts.Data;

namespace TopKPosts.Likes.Producer;

public class LikesProducer(IDbContextFactory<AppDbContext> appDbContext, IProducer<string, string> producer) : BackgroundService
{
    private readonly AppDbContext _appDbContext = appDbContext.CreateDbContext();

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var posts = await _appDbContext.Posts
                .AsNoTracking()
                .OrderBy(_ => Guid.NewGuid())
                .Take(100)
                .ToListAsync(stoppingToken);

            foreach (var post in posts)
            {
                await producer.ProduceAsync("likes-topic", new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = JsonSerializer.Serialize(new Like()
                    {
                        PostId = post.Id,
                        LikedAt = DateTime.UtcNow
                    })
                }, stoppingToken);
            }

            producer.Flush(stoppingToken);

            await Task.Delay(TimeSpan.FromMilliseconds(200), stoppingToken);
        }
    }
}
