using NewsAggregator.Application.Common;
using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Helpers;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Mappers;
using NewsAggregator.Domain.Interfaces;

namespace NewsAggregator.Application.Services
{
    public class NewsAppService : INewsAppService
    {
        private readonly INewsRepository _newsRepository;
        private readonly IArticleEventPublisher _articleEventPublisher;

        public NewsAppService(INewsRepository newsRepository, IArticleEventPublisher articleEventPublisher)
        {
            _newsRepository = newsRepository;
            _articleEventPublisher = articleEventPublisher;
        }

        public async Task<List<ArticleDto>> GetAllNews()
        {
            var objs = await _newsRepository.GetAllAsync();
            return ArticleMapper.ToDtoList(objs);
        }

        public async Task<Result<ArticleDto>> GetNewsById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Result<ArticleDto>.Fail("Invalid ID format.");

                if (!IdValidationHelper.IsValidHexadecimalId(id))
                    return Result<ArticleDto>.Fail("Invalid ID format.");

                var obj = await _newsRepository.GetNewsByIdAsync(id);
                if (obj is null)
                    return Result<ArticleDto>.Fail("Not found!");

                return Result<ArticleDto>.Ok(ArticleMapper.ToDto(obj!)!);
            }
            catch (Exception ex)
            {
                return Result<ArticleDto>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<ArticleDto>> CreateArticle(CreateArticleDto? input)
        {
            try
            {
                if (input is null)
                    return Result<ArticleDto>.Fail("Article is null.");

                if (string.IsNullOrWhiteSpace(input.Author) ||
                    string.IsNullOrWhiteSpace(input.Title) ||
                    string.IsNullOrWhiteSpace(input.Content))
                {
                    return Result<ArticleDto>.Fail("Author, Title, and Content cannot be empty or whitespace.");
                }

                var article = ArticleMapper.ToEntity(input);
                if (article is null)
                    return Result<ArticleDto>.Fail("Invalid article data");

                var returnArticle = await _newsRepository.AddAsync(article);
                if (returnArticle is null)
                    return Result<ArticleDto>.Fail("Failed to save article");

                var dto = ArticleMapper.ToDto(returnArticle);

                var articlePublishedEvent = ArticleMapper.ToEvent(returnArticle);

                _articleEventPublisher.PublishArticlePublished(articlePublishedEvent!);

                return Result<ArticleDto>.Ok(dto!);
            }
            catch (Exception ex)
            {
                return Result<ArticleDto>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<long>> UpdateArticle(string id, UpdateArticleDto input)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Result<long>.Fail("ID cannot be empty or null.");

                if (!IdValidationHelper.IsValidHexadecimalId(id))
                    return Result<long>.Fail("Invalid ID format.");

                if (string.IsNullOrWhiteSpace(input.Author) ||
                   string.IsNullOrWhiteSpace(input.Title) ||
                   string.IsNullOrWhiteSpace(input.Content))
                {
                    return Result<long>.Fail("Author, Title, and Content cannot be empty or whitespace.");
                }

                var obj = await _newsRepository.GetNewsByIdAsync(id);
                if (obj is null)
                    return Result<long>.Fail("Not found!");

                var article = ArticleMapper.ToEntity(input);
                if (article is null)
                    return Result<long>.Fail("Invalid article data");

                var updatedCount = await _newsRepository.UpdateAsync(id, article);
                if (updatedCount == 0)
                    return Result<long>.Fail("Failed to update article");

                return Result<long>.Ok(updatedCount);
            }
            catch (Exception ex)
            {
                return Result<long>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<long>> DeleteArticle(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Result<long>.Fail("ID cannot be empty or null.");

                if (!IdValidationHelper.IsValidHexadecimalId(id))
                    return Result<long>.Fail("Invalid ID format.");

                var obj = await _newsRepository.GetNewsByIdAsync(id);
                if (obj is null)
                    return Result<long>.Fail("Not found!");

                var deleteCount = await _newsRepository.DeleteAsync(id);
                if (deleteCount == 0)
                    return Result<long>.Fail("Failed to delete article");

                return Result<long>.Ok(deleteCount);
            }
            catch (Exception ex)
            {
                return Result<long>.Fail($"An error occurred: {ex.Message}");
            }
        }
    }
}