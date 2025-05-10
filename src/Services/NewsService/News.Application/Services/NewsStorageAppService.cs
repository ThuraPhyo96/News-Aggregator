using News.Domain.Interfaces;
using News.Domain.Models;

namespace News.Application.Services
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
