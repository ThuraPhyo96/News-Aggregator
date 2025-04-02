using NewsAggregator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregator.Application.Interfaces
{
    public interface INewsRepository
    {
        Task<List<Article>> GetAllAsync();
        Task<Article?> GetNewsByIdAsync(string id);
        Task AddAsync(Article news);
        //Task UpdateAsync(Article news);
        //Task DeleteAsync(string id);
    }
}
