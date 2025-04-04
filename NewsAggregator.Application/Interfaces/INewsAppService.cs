using NewsAggregator.Application.DTOs;
using NewsAggregator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregator.Application.Interfaces
{
    public interface INewsAppService
    {
        Task<List<ArticleDto>> GetAllNews();
        Task<ArticleDto> GetNewsById(string id);
    }
}
