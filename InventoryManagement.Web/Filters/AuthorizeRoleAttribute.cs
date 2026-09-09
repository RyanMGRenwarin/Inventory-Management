using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Filters;

/// <summary>
/// Attribute for applying role-based authorization.
/// </summary>
public class AuthorizeRoleAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the AuthorizeRoleAttribute class.
    /// </summary>
    /// <param name="requiredRole">The required role for access.</param>
    public AuthorizeRoleAttribute(UserRole requiredRole)
        : base(typeof(AuthorizeRoleFilter))
    {
        Arguments = new object[] { requiredRole };
    }
}
