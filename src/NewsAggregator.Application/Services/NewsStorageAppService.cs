using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Application.Services
{
    public class NewsStorageAppService
    {
        private readonly INewsRepository _newsRepository;

        public NewsStorageAppService(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task StoreArticlesAsync(List<Article> articles)
        {
            foreach (var article in articles)
            {
                await _newsRepository.AddAsync(article);
            }
        }
    }
}
