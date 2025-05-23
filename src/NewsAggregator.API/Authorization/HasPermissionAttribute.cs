﻿using Microsoft.AspNetCore.Authorization;

namespace NewsAggregator.API.Authorization
{
    // A simple custom attribute that ties a controller action to a permission policy.
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }
}
