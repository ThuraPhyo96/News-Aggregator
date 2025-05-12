using Microsoft.AspNetCore.Authorization;

namespace Users.API.Authorization
{
    // Defines what a permission requirement is.
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
