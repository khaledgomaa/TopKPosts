using Microsoft.EntityFrameworkCore;
using TopKPosts.Data;
using TopKPosts.Redis;

namespace TopKPosts.ApiService.Services
{
    public class LikesWorker(IDbContextFactory<AppDbContext> appDbContext, IRedisCacheService redisCacheService) : BackgroundService
    {
        private readonly AppDbContext _appDbContext = appDbContext.CreateDbContext();
        private readonly IRedisCacheService _redisCacheService = redisCacheService;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var posts = await _appDbContext.Posts
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var post in posts)
                {
                    var like = new Like
                    {
                        PostId = post.Id,
                        LikedAt = DateTime.UtcNow
                    };

                    await _appDbContext.Likes.AddAsync(like);
                }

                await _appDbContext.SaveChangesAsync(stoppingToken);

                foreach (var post in posts)
                {
                    await _redisCacheService.AddLikeAsync(post.Id);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1), stoppingToken);
            }
        }
    }
}
