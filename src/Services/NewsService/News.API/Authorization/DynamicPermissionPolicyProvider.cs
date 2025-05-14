using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using News.Domain.Interfaces;
using NewsAggregator.API.Authorization;

namespace News.API.Authorization
{
    public class DynamicPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "Permission:";
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private static readonly string CacheKey = "AllPermissions";

        public DynamicPermissionPolicyProvider(IServiceProvider serviceProvider, IOptions<AuthorizationOptions> options, IMemoryCache cache)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName[POLICY_PREFIX.Length..];

                // Try get cached permissions
                if (!_cache.TryGetValue(CacheKey, out HashSet<string>? permissions))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var permissionRepo = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

                    var fetchedPermissions = await permissionRepo.GetAllPermissionsAsync();
                    permissions = fetchedPermissions!.ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set(CacheKey, permissions, cacheEntryOptions);
                }

                if (permissions!.Contains(permission))
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new PermissionRequirement(permission))
                        .Build();

                    return policy;
                }

                return null; // policy not found
            }

            return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}