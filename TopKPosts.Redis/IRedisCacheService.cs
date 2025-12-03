
namespace TopKPosts.Redis
{
    public interface IRedisCacheService
    {
        Task<long> AddLikeAsync(int postId);
        Task<IReadOnlyList<(string PostId, long Likes)>> GetTopPostsAsync(int k);
    }
}