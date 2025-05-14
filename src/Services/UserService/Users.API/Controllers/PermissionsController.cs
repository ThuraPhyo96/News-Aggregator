using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Contracts.DTOs;
using Users.Application.Interfaces;

namespace Users.API.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionAppService _permissionAppService;

        public PermissionsController(IPermissionAppService permissionAppService)
        {
            _permissionAppService = permissionAppService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> Get()
        {
            var permissions = await _permissionAppService.GetAllPermissionClaimsAsync();
            return Ok(permissions.Select(p => new PermissionDto { Name = p }));
        }
    }
}
