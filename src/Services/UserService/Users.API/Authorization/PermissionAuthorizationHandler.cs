using Microsoft.AspNetCore.Authorization;

namespace Users.API.Authorization
{
    // This is what actually checks whether the current user has the permission claim.
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var hasClaim = context.User.Claims.Any(c => c.Type == "permission" &&
                                                   c.Value == requirement.Permission);

            if (hasClaim)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
