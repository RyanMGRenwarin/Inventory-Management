using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CurrentOccupancy = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinimumStockThreshold = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Electronic devices and accessories", "Electronics", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Apparel and fashion items", "Clothing", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Food products and drinks", "Food & Beverages", null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Home and office furniture", "Furniture", null },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Books and publications", "Books", null },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cosmetics and health products", "Beauty & Health", null },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sports equipment and accessories", "Sports", null },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Children's toys and games", "Toys", null },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Car and motorcycle accessories", "Automotive", null },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Home decor and kitchenware", "Home & Living", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginDate", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@inventory.com", "System Administrator", true, null, "$2a$11$0hdhguLaQ/ChcFqXUnqL5u21QIVRAXRkebMZy/FBJqWMAM.V4Blcy", 2, null, "admin" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "user@inventory.com", "Regular User", true, null, "$2a$11$Rc0hTySIZnnB8s/PBQzZGehZk7MHm.xqGkuLHa70WoDNsA25xUise", 1, null, "user" }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Capacity", "CreatedAt", "Location", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1000, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jakarta, Indonesia", "Main Warehouse", null },
                    { 2, 500, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bandung, Indonesia", "Secondary Warehouse", null },
                    { 3, 750, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Surabaya, Indonesia", "Surabaya Warehouse", null },
                    { 4, 400, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medan, Indonesia", "Medan Warehouse", null },
                    { 5, 600, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Makassar, Indonesia", "Makassar Warehouse", null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "MinimumStockThreshold", "Name", "Price", "SKU", "StockQuantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "6.7 inch OLED, 128GB storage, 8GB RAM", 10, "Smartphone X Pro", 899.99m, "PHN-001", 50, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "14 inch, Intel i7, 16GB RAM, 512GB SSD", 5, "Laptop UltraBook Z", 1299.99m, "LAP-001", 30, null },
                    { 3, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bluetooth 5.2, noise canceling, IPX5 water resistant", 20, "Wireless Earbuds", 89.99m, "AUD-001", 100, null },
                    { 4, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GPS, heart rate monitor, sleep tracking", 15, "Smartwatch S5", 249.99m, "WCH-001", 75, null },
                    { 5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "100% cotton, slim fit, various colors", 30, "Classic T-Shirt", 19.99m, "CLT-001", 200, null },
                    { 6, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Straight cut, 98% cotton 2% elastane", 20, "Denim Jeans", 49.99m, "CLT-002", 150, null },
                    { 7, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Waterproof, insulated, removable hood", 10, "Winter Jacket", 89.99m, "CLT-003", 80, null },
                    { 8, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arabica blend, medium roast, 1kg package", 25, "Premium Coffee Beans", 24.99m, "FOD-001", 120, null },
                    { 9, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "50 tea bags, organic certified", 40, "Organic Green Tea", 12.99m, "FOD-002", 200, null },
                    { 10, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "70% cocoa, fair trade", 50, "Dark Chocolate Bar", 4.99m, "FOD-003", 300, null },
                    { 11, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ergonomic, height adjustable, lumbar support", 5, "Office Chair", 199.99m, "FRN-001", 40, null },
                    { 12, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wooden dining table, seats 6 people", 5, "Dining Table", 349.99m, "FRN-002", 25, null },
                    { 13, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bestseller fiction, 400 pages", 10, "The Great Novel", 15.99m, "BOK-001", 60, null },
                    { 14, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "500 recipes, full color photography", 8, "Cooking Guide", 29.99m, "BOK-002", 45, null },
                    { 15, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "50ml, moisturizing, anti-aging", 15, "Organic Face Cream", 34.99m, "BTH-001", 90, null },
                    { 16, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30ml, 20% vitamin C, hyaluronic acid", 10, "Vitamin C Serum", 44.99m, "BTH-002", 70, null },
                    { 17, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Official size 7, rubber exterior", 20, "Basketball", 29.99m, "SPT-001", 110, null },
                    { 18, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lightweight, cushioned sole, breathable mesh", 15, "Running Shoes", 79.99m, "SPT-002", 95, null },
                    { 19, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "200 pieces, compatible with major brands", 25, "Building Blocks Set", 39.99m, "TOY-001", 130, null },
                    { 20, 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "12 inch doll with accessories", 15, "Doll Playset", 49.99m, "TOY-002", 85, null }
                });

            migrationBuilder.InsertData(
                table: "InventoryTransactions",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Notes", "ProductId", "Quantity", "TotalAmount", "TransactionDate", "Type", "UnitPrice", "UpdatedAt", "WarehouseId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 1, 50, 44999.50m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 899.99m, null, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 2, 30, 38999.70m, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1299.99m, null, 1 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 3, 100, 8999.00m, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 1, 89.99m, null, 2 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 5, 200, 3998.00m, new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 1, 19.99m, null, 2 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 8, 120, 2998.80m, new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 1, 24.99m, null, 3 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 11, 40, 7999.60m, new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), 1, 199.99m, null, 3 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 13, 60, 959.40m, new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), 1, 15.99m, null, 4 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 15, 90, 3149.10m, new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), 1, 34.99m, null, 4 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 17, 110, 3298.90m, new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), 1, 29.99m, null, 5 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Initial stock", 19, 130, 5198.70m, new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1, 39.99m, null, 5 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Customer order #1001", 1, 5, 4499.95m, new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, 899.99m, null, 1 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Customer order #1002", 3, 15, 1349.85m, new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), 2, 89.99m, null, 2 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Customer order #1003", 5, 30, 599.70m, new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), 2, 19.99m, null, 2 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Customer order #1004", 8, 10, 249.90m, new DateTime(2024, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), 2, 24.99m, null, 3 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Customer order #1005", 11, 3, 599.97m, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 199.99m, null, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CreatedByUserId",
                table: "InventoryTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId_TransactionDate",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionDate",
                table: "InventoryTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_Type",
                table: "InventoryTransactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_WarehouseId_TransactionDate",
                table: "InventoryTransactions",
                columns: new[] { "WarehouseId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Name",
                table: "Warehouses",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
