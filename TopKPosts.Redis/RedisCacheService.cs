using StackExchange.Redis;

namespace TopKPosts.Redis
{
    public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
    {
        private readonly IDatabase _db = redis.GetDatabase();
        private const string LikesSortedSetKey = "posts:likes";

        /// <summary>
        /// Increment the like count for a specific post.
        /// </summary>
        public async Task<long> AddLikeAsync(int postId)
        {
            // Increment the score (likes count) of the post in the sorted set
            double newScore = await _db.SortedSetIncrementAsync(LikesSortedSetKey, postId, 1);
            return (long)newScore;
        }

        /// <summary>
        /// Retrieve the top k posts by like count.
        /// </summary>
        public async Task<IReadOnlyList<(int PostId, long Likes)>> GetTopPostsAsync(int k)
        {
            // Get top k posts with highest scores
            var entries = await _db.SortedSetRangeByRankWithScoresAsync(
                LikesSortedSetKey,
                start: 0,
                stop: k - 1,
                order: Order.Descending);

            var result = new List<(int, long)>();

            foreach (var entry in entries)
            {
                result.Add(((int)entry.Element, (long)entry.Score));
            }

            return result;
        }
    }
}