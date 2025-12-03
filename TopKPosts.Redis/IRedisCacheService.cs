
namespace TopKPosts.Redis
{
    public interface IRedisCacheService
    {
        Task<long> AddLikeAsync(int postId);
        Task<IReadOnlyList<(int PostId, long Likes)>> GetTopPostsAsync(int k);
    }
}