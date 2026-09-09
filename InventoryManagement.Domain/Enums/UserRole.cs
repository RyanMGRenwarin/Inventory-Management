namespace InventoryManagement.Domain.Enums
{
    public enum UserRole
    {
        /// <summary>
        /// Regular user with view and transaction creation permissions.
        /// </summary>
        User = 1,

        /// <summary>
        /// Administrator with full system access.
        /// </summary>
        Admin = 2
    }
}
