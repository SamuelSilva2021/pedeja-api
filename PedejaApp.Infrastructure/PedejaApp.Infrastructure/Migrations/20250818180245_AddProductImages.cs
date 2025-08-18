using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PedejaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryTime",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedPreparationMinutes",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueuePosition",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderRejections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RejectedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRejections_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    AspectRatio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MediumPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LargePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2361), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2520) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2657), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2658) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2660), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2661) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2663), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(2663) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8953), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8953) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8960), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8961) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8987), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8987) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8990), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8990) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8992), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8992) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8994), new DateTime(2025, 8, 18, 18, 2, 43, 357, DateTimeKind.Utc).AddTicks(8995) });

            migrationBuilder.CreateIndex(
                name: "IX_OrderRejections_OrderId",
                table: "OrderRejections",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderRejections_RejectedAt",
                table: "OrderRejections",
                column: "RejectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderId",
                table: "OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_Status",
                table: "OrderStatusHistory",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_Timestamp",
                table: "OrderStatusHistory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_IsDeleted",
                table: "ProductImages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_IsPrimary",
                table: "ProductImages",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_UploadDate",
                table: "ProductImages",
                column: "UploadDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderRejections");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedPreparationMinutes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QueuePosition",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1374), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1521) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1660), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1660) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1663), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1663) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1666), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(1666) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8179), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8180) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8185), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8186) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8188), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8189) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8191), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8192) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8194), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8194) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8196), new DateTime(2025, 8, 16, 16, 26, 6, 766, DateTimeKind.Utc).AddTicks(8197) });
        }
    }
}
