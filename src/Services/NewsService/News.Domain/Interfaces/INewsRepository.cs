using News.Domain.Models;

namespace News.Domain.Interfaces
{
    public interface INewsRepository
    {
        Task<List<Article>> GetAllAsync();
        Task<Article?> GetNewsByIdAsync(string id);
        Task<Article> AddAsync(Article news);
        Task<long> UpdateAsync(string id, Article article);
        Task<long> DeleteAsync(string id);
    }
}
