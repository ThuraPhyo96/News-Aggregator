using Users.Application.Interfaces;
using Users.Domain.Authorization;

namespace Users.Application.Services
{
    public class PermissionAppService : IPermissionAppService
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionAppService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<List<string>> GetAllPermissionClaimsAsync()
        {
            return await _permissionRepository.GetAllPermissionClaimsAsync();
        }
    }
}
