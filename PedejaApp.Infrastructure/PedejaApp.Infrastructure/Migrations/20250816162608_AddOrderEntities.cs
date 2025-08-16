using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PedejaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDelivery = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 10, scale: 2, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1374), "Pratos principais do cardápio", 1, true, false, "Pratos Principais", new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1521) },
                    { 2, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1660), "Bebidas e refrescos", 2, true, false, "Bebidas", new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1660) },
                    { 3, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1663), "Doces e sobremesas", 3, true, false, "Sobremesas", new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1663) },
                    { 4, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1666), "Aperitivos e entradas", 4, true, false, "Entradas", new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1666) }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "IsDeleted", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8179), "Hambúrguer artesanal com carne bovina, queijo, alface, tomate e molho especial", 1, null, true, false, "Hambúrguer Clássico", 25.90m, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8180) },
                    { 2, 1, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8185), "Pizza tradicional com molho de tomate, mussarela e manjericão fresco", 2, null, true, false, "Pizza Margherita", 32.50m, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8186) },
                    { 3, 2, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8188), "Refrigerante gelado em lata de 350ml - diversos sabores", 1, null, true, false, "Refrigerante Lata", 5.50m, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8189) },
                    { 4, 2, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8191), "Suco natural de frutas frescas - 400ml", 2, null, true, false, "Suco Natural", 8.90m, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8192) },
                    { 5, 3, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8194), "Pudim caseiro de leite condensado com calda de açúcar", 1, null, true, false, "Pudim de Leite", 12.90m, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8194) },
                    { 6, 4, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8196), "Batatas crocantes cortadas na casa, tempero especial - serve até 3 pessoas", 1, null, true, false, "Porção de Batata Frita", 18.50m, new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8197) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerPhone",
                table: "Orders",
                column: "CustomerPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DisplayOrder",
                table: "Products",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
