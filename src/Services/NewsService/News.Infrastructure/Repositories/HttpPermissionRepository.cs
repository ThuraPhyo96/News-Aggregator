using Microsoft.Extensions.Logging;
using News.Domain.Interfaces;
using NewsAggregator.Contracts.DTOs;
using System.Net.Http.Json;

namespace News.Infrastructure.Repositories
{
    public class HttpPermissionRepository : IPermissionRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpPermissionRepository> _logger;

        public HttpPermissionRepository(HttpClient httpClient, ILogger<HttpPermissionRepository> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var permissions = await _httpClient.GetFromJsonAsync<List<PermissionDto>>(
                    "api/permissions", cancellationToken);

                return permissions?.Select(p => p.Name) ?? Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch permissions from UsersAPI.");
                return Enumerable.Empty<string>();
            }
        }
    }
}
