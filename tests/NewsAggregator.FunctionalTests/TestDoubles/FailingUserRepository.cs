using MongoDB.Driver;
using NewsAggregator.Application.Common;

namespace NewsAggregator.FunctionalTests.TestDoubles
{
    public class FailingUserRepository
    {
        public Task<Result<long>> GetByUsername(string username)
            => throw new MongoException("Simulated MongoDB failure");

        public Task<Result<long>> CreateUser(string username)
         => throw new MongoException("Simulated MongoDB failure");
    }
}
