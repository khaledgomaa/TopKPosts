using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TopKPosts.Data;

namespace TopKPosts.ApiService.Consumers
{
    public class KafkaRouterConsumer(IDbContextFactory<AppDbContext> dbContextFactory, IConsumer<string, string> consumer) : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
        private readonly IConsumer<string, string> _consumer = consumer;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(["posts-topic", "likes-topic"]);

            Console.WriteLine($"KafkaRouterConsumer subscribes to {string.Join(',', _consumer.Subscription)}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult is null)
                    {
                        continue;
                    }

                    Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' from '{consumeResult.Topic}' at: '{consumeResult.TopicPartitionOffset}'.");

                    // Create a short-lived DbContext per message to follow recommended EF patterns.
                    await using var dbContext = _dbContextFactory.CreateDbContext();

                    switch (consumeResult.Topic)
                    {
                        case "posts-topic":
                            var post = JsonSerializer.Deserialize<Post>(consumeResult.Message.Value);
                            if (post != null)
                            {
                                await dbContext.AddAsync(post, stoppingToken);
                                await dbContext.SaveChangesAsync(stoppingToken);
                            }
                            break;

                        case "likes-topic":
                            var like = JsonSerializer.Deserialize<Like>(consumeResult.Message.Value);
                            if (like != null)
                            {
                                await dbContext.AddAsync(like, stoppingToken);
                                await dbContext.SaveChangesAsync(stoppingToken);
                            }
                            break;

                        default:
                            Console.WriteLine($"Unknown topic: {consumeResult.Topic}");
                            break;
                    }

                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    // shutting down
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), stoppingToken);
                }
            }
        }
    }
}
