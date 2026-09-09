using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core database context for the application.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the AppDbContext class.
    /// </remarks>
    /// <param name="options">DbContext options.</param>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets or sets the products DbSet.
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the warehouses DbSet.
        /// </summary>
        public DbSet<Warehouse> Warehouses { get; set; }

        /// <summary>
        /// Gets or sets the inventory transactions DbSet.
        /// </summary>
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        /// <summary>
        /// Gets or sets the categories DbSet.
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the users DbSet.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the refresh tokens DbSet.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Configures the entity models.
        /// </summary>
        /// <param name="modelBuilder">Model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Configure global query filters
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Warehouse>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<InventoryTransaction>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.IsDeleted && rt.User != null && !rt.User.IsDeleted);

            // Seed initial data
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seeds initial data for the database.
        /// </summary>
        /// <param name="modelBuilder">Model builder instance.</param>
        private static void SeedData(ModelBuilder modelBuilder)
        {
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // ============================================================
            // SEED USERS
            // ============================================================
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@inventory.com",
                    PasswordHash = "$2a$11$ZY4Qy5EZ5r2U5.kFQ8z1X.N5GZ5UVMLqH.8ZQy5EZ5r2U5.kFQ8z1X",
                    FullName = "System Administrator",
                    Role = Domain.Enums.UserRole.Admin,
                    IsActive = true,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new User
                {
                    Id = 2,
                    Username = "user",
                    Email = "user@inventory.com",
                    PasswordHash = "$2a$11$M4Qy5EZ5r2U5.kFQ8z1X.N5GZ5UVMLqH.8ZQy5EZ5r2U5.kFQ8z1X",
                    FullName = "Regular User",
                    Role = Domain.Enums.UserRole.User,
                    IsActive = true,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                }
            );

            // ============================================================
            // SEED CATEGORIES
            // ============================================================
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 3, Name = "Food & Beverages", Description = "Food products and drinks", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 4, Name = "Furniture", Description = "Home and office furniture", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 5, Name = "Books", Description = "Books and publications", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 6, Name = "Beauty & Health", Description = "Cosmetics and health products", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 7, Name = "Sports", Description = "Sports equipment and accessories", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 8, Name = "Toys", Description = "Children's toys and games", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 9, Name = "Automotive", Description = "Car and motorcycle accessories", CreatedAt = fixedDate, IsDeleted = false },
                new Category { Id = 10, Name = "Home & Living", Description = "Home decor and kitchenware", CreatedAt = fixedDate, IsDeleted = false }
            );

            // ============================================================
            // SEED WAREHOUSES
            // ============================================================
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Main Warehouse", Location = "Jakarta, Indonesia", Capacity = 1000, CurrentOccupancy = 0, CreatedAt = fixedDate, IsDeleted = false },
                new Warehouse { Id = 2, Name = "Secondary Warehouse", Location = "Bandung, Indonesia", Capacity = 500, CurrentOccupancy = 0, CreatedAt = fixedDate, IsDeleted = false },
                new Warehouse { Id = 3, Name = "Surabaya Warehouse", Location = "Surabaya, Indonesia", Capacity = 750, CurrentOccupancy = 0, CreatedAt = fixedDate, IsDeleted = false },
                new Warehouse { Id = 4, Name = "Medan Warehouse", Location = "Medan, Indonesia", Capacity = 400, CurrentOccupancy = 0, CreatedAt = fixedDate, IsDeleted = false },
                new Warehouse { Id = 5, Name = "Makassar Warehouse", Location = "Makassar, Indonesia", Capacity = 600, CurrentOccupancy = 0, CreatedAt = fixedDate, IsDeleted = false }
            );

            // ============================================================
            // SEED PRODUCTS
            // ============================================================
            modelBuilder.Entity<Product>().HasData(
                // Electronics (Category 1)
                new Product { Id = 1, Name = "Smartphone X Pro", SKU = "PHN-001", Description = "6.7 inch OLED, 128GB storage, 8GB RAM", Price = 899.99m, StockQuantity = 50, MinimumStockThreshold = 10, CategoryId = 1, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 2, Name = "Laptop UltraBook Z", SKU = "LAP-001", Description = "14 inch, Intel i7, 16GB RAM, 512GB SSD", Price = 1299.99m, StockQuantity = 30, MinimumStockThreshold = 5, CategoryId = 1, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 3, Name = "Wireless Earbuds", SKU = "AUD-001", Description = "Bluetooth 5.2, noise canceling, IPX5 water resistant", Price = 89.99m, StockQuantity = 100, MinimumStockThreshold = 20, CategoryId = 1, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 4, Name = "Smartwatch S5", SKU = "WCH-001", Description = "GPS, heart rate monitor, sleep tracking", Price = 249.99m, StockQuantity = 75, MinimumStockThreshold = 15, CategoryId = 1, CreatedAt = fixedDate, IsDeleted = false },

                // Clothing (Category 2)
                new Product { Id = 5, Name = "Classic T-Shirt", SKU = "CLT-001", Description = "100% cotton, slim fit, various colors", Price = 19.99m, StockQuantity = 200, MinimumStockThreshold = 30, CategoryId = 2, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 6, Name = "Denim Jeans", SKU = "CLT-002", Description = "Straight cut, 98% cotton 2% elastane", Price = 49.99m, StockQuantity = 150, MinimumStockThreshold = 20, CategoryId = 2, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 7, Name = "Winter Jacket", SKU = "CLT-003", Description = "Waterproof, insulated, removable hood", Price = 89.99m, StockQuantity = 80, MinimumStockThreshold = 10, CategoryId = 2, CreatedAt = fixedDate, IsDeleted = false },

                // Food & Beverages (Category 3)
                new Product { Id = 8, Name = "Premium Coffee Beans", SKU = "FOD-001", Description = "Arabica blend, medium roast, 1kg package", Price = 24.99m, StockQuantity = 120, MinimumStockThreshold = 25, CategoryId = 3, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 9, Name = "Organic Green Tea", SKU = "FOD-002", Description = "50 tea bags, organic certified", Price = 12.99m, StockQuantity = 200, MinimumStockThreshold = 40, CategoryId = 3, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 10, Name = "Dark Chocolate Bar", SKU = "FOD-003", Description = "70% cocoa, fair trade", Price = 4.99m, StockQuantity = 300, MinimumStockThreshold = 50, CategoryId = 3, CreatedAt = fixedDate, IsDeleted = false },

                // Furniture (Category 4)
                new Product { Id = 11, Name = "Office Chair", SKU = "FRN-001", Description = "Ergonomic, height adjustable, lumbar support", Price = 199.99m, StockQuantity = 40, MinimumStockThreshold = 5, CategoryId = 4, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 12, Name = "Dining Table", SKU = "FRN-002", Description = "Wooden dining table, seats 6 people", Price = 349.99m, StockQuantity = 25, MinimumStockThreshold = 5, CategoryId = 4, CreatedAt = fixedDate, IsDeleted = false },

                // Books (Category 5)
                new Product { Id = 13, Name = "The Great Novel", SKU = "BOK-001", Description = "Bestseller fiction, 400 pages", Price = 15.99m, StockQuantity = 60, MinimumStockThreshold = 10, CategoryId = 5, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 14, Name = "Cooking Guide", SKU = "BOK-002", Description = "500 recipes, full color photography", Price = 29.99m, StockQuantity = 45, MinimumStockThreshold = 8, CategoryId = 5, CreatedAt = fixedDate, IsDeleted = false },

                // Beauty & Health (Category 6)
                new Product { Id = 15, Name = "Organic Face Cream", SKU = "BTH-001", Description = "50ml, moisturizing, anti-aging", Price = 34.99m, StockQuantity = 90, MinimumStockThreshold = 15, CategoryId = 6, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 16, Name = "Vitamin C Serum", SKU = "BTH-002", Description = "30ml, 20% vitamin C, hyaluronic acid", Price = 44.99m, StockQuantity = 70, MinimumStockThreshold = 10, CategoryId = 6, CreatedAt = fixedDate, IsDeleted = false },

                // Sports (Category 7)
                new Product { Id = 17, Name = "Basketball", SKU = "SPT-001", Description = "Official size 7, rubber exterior", Price = 29.99m, StockQuantity = 110, MinimumStockThreshold = 20, CategoryId = 7, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 18, Name = "Running Shoes", SKU = "SPT-002", Description = "Lightweight, cushioned sole, breathable mesh", Price = 79.99m, StockQuantity = 95, MinimumStockThreshold = 15, CategoryId = 7, CreatedAt = fixedDate, IsDeleted = false },

                // Toys (Category 8)
                new Product { Id = 19, Name = "Building Blocks Set", SKU = "TOY-001", Description = "200 pieces, compatible with major brands", Price = 39.99m, StockQuantity = 130, MinimumStockThreshold = 25, CategoryId = 8, CreatedAt = fixedDate, IsDeleted = false },
                new Product { Id = 20, Name = "Doll Playset", SKU = "TOY-002", Description = "12 inch doll with accessories", Price = 49.99m, StockQuantity = 85, MinimumStockThreshold = 15, CategoryId = 8, CreatedAt = fixedDate, IsDeleted = false }
            );

            // ============================================================
            // SEED TRANSACTIONS
            // ============================================================
            modelBuilder.Entity<InventoryTransaction>().HasData(
                // Inbound transactions (Product 1-20, Warehouse 1-5)
                new InventoryTransaction
                {
                    Id = 1,
                    ProductId = 1,
                    WarehouseId = 1,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 50,
                    UnitPrice = 899.99m,
                    TotalAmount = 44999.50m,
                    TransactionDate = fixedDate,
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 2,
                    ProductId = 2,
                    WarehouseId = 1,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 30,
                    UnitPrice = 1299.99m,
                    TotalAmount = 38999.70m,
                    TransactionDate = fixedDate.AddDays(1),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 3,
                    ProductId = 3,
                    WarehouseId = 2,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 100,
                    UnitPrice = 89.99m,
                    TotalAmount = 8999.00m,
                    TransactionDate = fixedDate.AddDays(2),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 4,
                    ProductId = 5,
                    WarehouseId = 2,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 200,
                    UnitPrice = 19.99m,
                    TotalAmount = 3998.00m,
                    TransactionDate = fixedDate.AddDays(3),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 5,
                    ProductId = 8,
                    WarehouseId = 3,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 120,
                    UnitPrice = 24.99m,
                    TotalAmount = 2998.80m,
                    TransactionDate = fixedDate.AddDays(4),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 6,
                    ProductId = 11,
                    WarehouseId = 3,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 40,
                    UnitPrice = 199.99m,
                    TotalAmount = 7999.60m,
                    TransactionDate = fixedDate.AddDays(5),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 7,
                    ProductId = 13,
                    WarehouseId = 4,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 60,
                    UnitPrice = 15.99m,
                    TotalAmount = 959.40m,
                    TransactionDate = fixedDate.AddDays(6),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 8,
                    ProductId = 15,
                    WarehouseId = 4,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 90,
                    UnitPrice = 34.99m,
                    TotalAmount = 3149.10m,
                    TransactionDate = fixedDate.AddDays(7),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 9,
                    ProductId = 17,
                    WarehouseId = 5,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 110,
                    UnitPrice = 29.99m,
                    TotalAmount = 3298.90m,
                    TransactionDate = fixedDate.AddDays(8),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 10,
                    ProductId = 19,
                    WarehouseId = 5,
                    Type = Domain.Enums.TransactionType.Inbound,
                    Quantity = 130,
                    UnitPrice = 39.99m,
                    TotalAmount = 5198.70m,
                    TransactionDate = fixedDate.AddDays(9),
                    Notes = "Initial stock",
                    CreatedByUserId = 1,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                // Outbound transactions
                new InventoryTransaction
                {
                    Id = 11,
                    ProductId = 1,
                    WarehouseId = 1,
                    Type = Domain.Enums.TransactionType.Outbound,
                    Quantity = 5,
                    UnitPrice = 899.99m,
                    TotalAmount = 4499.95m,
                    TransactionDate = fixedDate.AddDays(10),
                    Notes = "Customer order #1001",
                    CreatedByUserId = 2,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 12,
                    ProductId = 3,
                    WarehouseId = 2,
                    Type = Domain.Enums.TransactionType.Outbound,
                    Quantity = 15,
                    UnitPrice = 89.99m,
                    TotalAmount = 1349.85m,
                    TransactionDate = fixedDate.AddDays(11),
                    Notes = "Customer order #1002",
                    CreatedByUserId = 2,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 13,
                    ProductId = 5,
                    WarehouseId = 2,
                    Type = Domain.Enums.TransactionType.Outbound,
                    Quantity = 30,
                    UnitPrice = 19.99m,
                    TotalAmount = 599.70m,
                    TransactionDate = fixedDate.AddDays(12),
                    Notes = "Customer order #1003",
                    CreatedByUserId = 2,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 14,
                    ProductId = 8,
                    WarehouseId = 3,
                    Type = Domain.Enums.TransactionType.Outbound,
                    Quantity = 10,
                    UnitPrice = 24.99m,
                    TotalAmount = 249.90m,
                    TransactionDate = fixedDate.AddDays(13),
                    Notes = "Customer order #1004",
                    CreatedByUserId = 2,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                },
                new InventoryTransaction
                {
                    Id = 15,
                    ProductId = 11,
                    WarehouseId = 3,
                    Type = Domain.Enums.TransactionType.Outbound,
                    Quantity = 3,
                    UnitPrice = 199.99m,
                    TotalAmount = 599.97m,
                    TransactionDate = fixedDate.AddDays(14),
                    Notes = "Customer order #1005",
                    CreatedByUserId = 2,
                    CreatedAt = fixedDate,
                    IsDeleted = false
                }
            );
        }
    }
}
