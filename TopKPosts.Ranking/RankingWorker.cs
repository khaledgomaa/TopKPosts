
using Confluent.Kafka;
using System.Text.Json;
using TopKPosts.Data;
using TopKPosts.Redis;

namespace TopKPosts.Ranking
{
    public class RankingWorker(IConsumer<string, string> consumer, IRedisCacheService redisCacheService) : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer = consumer;
        private readonly IRedisCacheService _redisCacheService = redisCacheService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("likes-topic");

            Console.WriteLine($"LikesConsumer subscribes to {string.Join(",", _consumer.ConsumerGroupMetadata)}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult != null)
                    {
                        Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartition.Partition}'.");

                        var like = JsonSerializer.Deserialize<Like>(consumeResult.Message.Value);

                        await _redisCacheService.AddLikeAsync(like!.PostId);

                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
            }
        }
    }
}
