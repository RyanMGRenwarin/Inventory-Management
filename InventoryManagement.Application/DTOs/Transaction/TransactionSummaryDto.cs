namespace InventoryManagement.Application.DTOs.Transaction
{
    /// <summary>
    /// DTO for transaction summary data.
    /// </summary>
    public class TransactionSummaryDto
    {
        /// <summary>
        /// Gets or sets the total number of transactions.
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// Gets or sets the total inbound quantity.
        /// </summary>
        public int TotalInboundQuantity { get; set; }

        /// <summary>
        /// Gets or sets the total outbound quantity.
        /// </summary>
        public int TotalOutboundQuantity { get; set; }

        /// <summary>
        /// Gets or sets the total transaction value.
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions today.
        /// </summary>
        public int TodayTransactions { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions this week.
        /// </summary>
        public int WeekTransactions { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions this month.
        /// </summary>
        public int MonthTransactions { get; set; }
    }
}
