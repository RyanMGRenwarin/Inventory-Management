using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration for the InventoryTransaction entity.
    /// </summary>
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        /// <summary>
        /// Configures the InventoryTransaction entity.
        /// </summary>
        /// <param name="builder">Entity type builder.</param>
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Quantity)
                .IsRequired();

            builder.Property(t => t.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(t => t.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(t => t.TransactionDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(t => t.Product)
                .WithMany(p => p.InventoryTransactions)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Warehouse)
                .WithMany(w => w.InventoryTransactions)
                .HasForeignKey(t => t.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CreatedByUser)
                .WithMany(u => u.InventoryTransactions)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.TransactionDate);
            builder.HasIndex(t => t.Type);
            builder.HasIndex(t => new { t.ProductId, t.TransactionDate });
            builder.HasIndex(t => new { t.WarehouseId, t.TransactionDate });
        }
    }
}
