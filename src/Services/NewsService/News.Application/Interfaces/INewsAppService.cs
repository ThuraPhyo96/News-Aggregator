using News.Application.Common;
using News.Application.DTOs;

namespace News.Application.Interfaces
{
    public interface INewsAppService
    {
        Task<List<ArticleDto>> GetAllNews();
        Task<Result<ArticleDto>> GetNewsById(string id);
        Task<Result<ArticleDto>> CreateArticle(CreateArticleDto? input);
        Task<Result<long>> UpdateArticle(string id, UpdateArticleDto input);
        Task<Result<long>> DeleteArticle(string id);
    }
}
