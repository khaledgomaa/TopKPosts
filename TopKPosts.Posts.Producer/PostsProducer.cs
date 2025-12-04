
using Bogus;
using Confluent.Kafka;
using TopKPosts.Data;

namespace TopKPosts.Posts.Producer;

public class PostsProducer(IProducer<string, string> producer) : BackgroundService
{
    private readonly Faker<Post> _postFaker = new Faker<Post>()
        .RuleFor(p => p.Title, f => f.Lorem.Sentence(5))
        .RuleFor(p => p.Content, f => f.Lorem.Paragraph())
        .RuleFor(p => p.CreatedAt, f => DateTime.UtcNow);

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var post = _postFaker.Generate();

            await producer.ProduceAsync("posts-topic", new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = System.Text.Json.JsonSerializer.Serialize(post)
            }, stoppingToken);

            producer.Flush(stoppingToken);

            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
        }
    }
}
