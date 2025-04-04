﻿using NewsAggregator.Application.Common;
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

        public async Task<Result<ArticleDto>> CreateArticle(CreateArticleDto input)
        {
            var article = ArticleMapper.ToEntity(input);
            if (article is null)
                return Result<ArticleDto>.Fail("Invalid article data");

            var returnArticle = await _newsRepository.AddAsync(article);
            if (returnArticle == null)
                return Result<ArticleDto>.Fail("Failed to save article");

            var dto = ArticleMapper.ToDto(returnArticle);
            return Result<ArticleDto>.Ok(dto!);
        }

        public async Task<Result<long>> UpdateArticle(string id, UpdateArticleDto input)
        {
            var article = ArticleMapper.ToEntity(input);
            if (article is null)
                return Result<long>.Fail("Invalid article data");

            var updatedCount = await _newsRepository.UpdateAsync(id, article);
            if (updatedCount == 0)
                return Result<long>.Fail("Failed to update article");

            return Result<long>.Ok(updatedCount);
        }

        public async Task<Result<long>> DeleteArticle(string id)
        {
            var deleteCount = await _newsRepository.DeleteAsync(id);
            if (deleteCount == 0)
                return Result<long>.Fail("Faled to delete article");

            return Result<long>.Ok(deleteCount);
        }
    }
}