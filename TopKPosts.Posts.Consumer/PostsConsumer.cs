using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TopKPosts.Data;

namespace TopKPosts.Posts.Consumer
{
    public class PostsConsumer(IDbContextFactory<AppDbContext> dbContextFactory, IConsumer<string, string> consumer) : BackgroundService
    {
        private readonly AppDbContext _dbContext = dbContextFactory.CreateDbContext();
        private readonly IConsumer<string, string> _consumer = consumer;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("posts-topic");

            Console.WriteLine($"PostsConsumer subscribes to {string.Join(",", _consumer.Subscription)}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult != null)
                    {
                        Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");

                        await _dbContext.AddAsync(
                            JsonSerializer.Deserialize<Post>(consumeResult.Message.Value)!,
                            stoppingToken);

                        await _dbContext.SaveChangesAsync(stoppingToken);

                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), stoppingToken);
                }
            }
        }
    }
}
