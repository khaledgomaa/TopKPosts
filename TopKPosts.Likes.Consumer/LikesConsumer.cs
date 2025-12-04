
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TopKPosts.Data;

namespace TopKPosts.Likes.Consumer
{
    public class LikesConsumer(IDbContextFactory<AppDbContext> dbContextFactory, IConsumer<string, string> consumer) : BackgroundService
    {
        private readonly AppDbContext _dbContext = dbContextFactory.CreateDbContext();
        private readonly IConsumer<string, string> _consumer = consumer;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("likes-topic");

            Console.WriteLine($"LikesConsumer subscribes to {string.Join(",", _consumer.Subscription)}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult != null)
                    {
                        Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");

                        await _dbContext.AddAsync(
                            JsonSerializer.Deserialize<Like>(consumeResult.Message.Value)!,
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
