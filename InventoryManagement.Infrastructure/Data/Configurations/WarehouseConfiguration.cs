using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration for the Warehouse entity.
    /// </summary>
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        /// <summary>
        /// Configures the Warehouse entity.
        /// </summary>
        /// <param name="builder">Entity type builder.</param>
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(w => w.Name)
                .IsUnique();

            builder.Property(w => w.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(w => w.Capacity)
                .IsRequired();

            builder.Property(w => w.CurrentOccupancy)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(w => w.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(w => w.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasMany(w => w.InventoryTransactions)
                .WithOne(t => t.Warehouse)
                .HasForeignKey(t => t.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
