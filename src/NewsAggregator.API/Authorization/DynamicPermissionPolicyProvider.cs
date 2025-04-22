using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NewsAggregator.Domain.Authorization;

namespace NewsAggregator.API.Authorization
{
    public class DynamicPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "Permission:";
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly IServiceProvider _serviceProvider;

        public DynamicPermissionPolicyProvider(IServiceProvider serviceProvider, IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _serviceProvider = serviceProvider;
        }

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                using var scope = _serviceProvider.CreateScope();
                var permissionRepo = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

                var permission = policyName[POLICY_PREFIX.Length..];

                var permissions = await permissionRepo.GetAllPermissionClaimsAsync();

                if (permissions.Contains(permission))
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