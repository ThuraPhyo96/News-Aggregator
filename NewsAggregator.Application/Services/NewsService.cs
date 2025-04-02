using NewsAggregator.Application.Interfaces;
using NewsAggregator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregator.Application.Services
{
    public class NewsService
    {
        private readonly INewsRepository _newsRepository;

        public NewsService(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task<List<Article>> GetAllNews()
        {
            return await _newsRepository.GetAllAsync();
        }
    }
}