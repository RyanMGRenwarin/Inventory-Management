namespace InventoryManagement.Domain.Enums
{
    public enum TransactionType
    {
        /// <summary>
        /// Stock inbound (receiving goods).
        /// </summary>
        Inbound = 1,

        /// <summary>
        /// Stock outbound (shipping goods).
        /// </summary>
        Outbound = 2
    }
}
