using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Mappers;
using NewsAggregator.Domain.Interfaces;

namespace NewsAggregator.Application.Services
{
    public class NewsAppService : INewsAppService
    {
        private readonly INewsRepository _newsRepository;

        public NewsAppService(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task<List<ArticleDto>> GetAllNews()
        {
            var objs = await _newsRepository.GetAllAsync();
            return ArticleMapper.ToDtoList(objs);
        }

        public async Task<ArticleDto> GetNewsById(string id)
        {
            var obj = await _newsRepository.GetNewsByIdAsync(id);
            if (obj is null) return new ArticleDto();

            return ArticleMapper.ToDto(obj!)!;
        }
    }
}