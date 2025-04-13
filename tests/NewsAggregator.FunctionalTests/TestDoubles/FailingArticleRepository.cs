using MongoDB.Driver;
using NewsAggregator.Application.Common;
using NewsAggregator.Application.DTOs;

namespace NewsAggregator.FunctionalTests.TestDoubles
{
    public class FailingArticleRepository
    {
        public Task<Result<long>> UpdateArticle(string id, UpdateArticleDto input)
            => throw new MongoException("Simulated MongoDB failure");

        public Task<Result<long>> DeleteArticle(string id)
             => throw new MongoException("Simulated MongoDB failure");
    }
}
