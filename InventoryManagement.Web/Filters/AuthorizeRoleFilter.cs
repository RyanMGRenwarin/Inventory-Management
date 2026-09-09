using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventoryManagement.Web.Filters
{
    /// <summary>
    /// Custom authorization filter for role-based access control.
    /// </summary>
    public class AuthorizeRoleFilter : IAuthorizationFilter
    {
        private readonly UserRole _requiredRole;

        /// <summary>
        /// Initializes a new instance of the AuthorizeRoleFilter class.
        /// </summary>
        /// <param name="requiredRole">The required role for access.</param>
        public AuthorizeRoleFilter(UserRole requiredRole)
        {
            _requiredRole = requiredRole;
        }

        /// <inheritdoc/>
        void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRole = user.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check if user has the required role or is Admin
            var hasRole = Enum.TryParse<UserRole>(userRole, out var role) &&
                          (role == _requiredRole || role == UserRole.Admin);

            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
