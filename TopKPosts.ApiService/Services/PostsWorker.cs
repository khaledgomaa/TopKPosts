
using Bogus;
using Microsoft.EntityFrameworkCore;
using TopKPosts.Data;

namespace TopKPosts.ApiService.Services
{
    public class PostsWorker(IDbContextFactory<AppDbContext> appDbContext) : BackgroundService
    {
        private readonly AppDbContext _appDbContext = appDbContext.CreateDbContext();
        private readonly Faker<Post> _postFaker = new Faker<Post>()
            .RuleFor(p => p.Title, f => f.Lorem.Sentence(5))
            .RuleFor(p => p.Content, f => f.Lorem.Paragraph())
            .RuleFor(p => p.CreatedAt, f => DateTime.UtcNow);

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var post = _postFaker.Generate();

                await _appDbContext.AddAsync(post);
                await _appDbContext.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }
    }
}
