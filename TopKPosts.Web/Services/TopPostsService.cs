using Microsoft.EntityFrameworkCore;
using TopKPosts.Data;
using TopKPosts.Redis;

namespace TopKPosts.Web.Services;

public class TopPostsService(IRedisCacheService redisCacheService, IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<List<PostWithLikes>> GetTopPostsAsync(int count = 10)
    {
        // Get top post IDs from Redis
        var topPostsFromRedis = await redisCacheService.GetTopPostsAsync(count);

        if (topPostsFromRedis.Count == 0)
        {
            return [];
        }

        var postIds = topPostsFromRedis
            .Select(x => x.PostId)
            .ToList();

        if (postIds.Count == 0)
        {
            return [];
        }

        // Get post data from database
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var posts = await dbContext.Posts
            .Where(p => postIds.Contains(p.Id))
            .ToListAsync();

        var postsDict = posts.ToDictionary(p => p.Id);

        var result = new List<PostWithLikes>();
        foreach (var (postIdStr, likes) in topPostsFromRedis)
        {
            if (postsDict.TryGetValue(postIdStr, out var post))
            {
                result.Add(new PostWithLikes
                {
                    Id = post.Id,
                    Title = post.Title ?? string.Empty,
                    Content = post.Content ?? string.Empty,
                    CreatedAt = post.CreatedAt,
                    Likes = likes
                });
            }
        }

        return result;
    }
}

public class PostWithLikes
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long Likes { get; set; }
}

