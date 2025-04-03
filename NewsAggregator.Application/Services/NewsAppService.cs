using NewsAggregator.Application.Interfaces;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Application.Services
{
    public class NewsAppService: INewsAppService
    {
        private readonly INewsRepository _newsRepository;

        public NewsAppService(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task<List<Article>> GetAllNews()
        {
            return await _newsRepository.GetAllAsync();
        }
    }
}