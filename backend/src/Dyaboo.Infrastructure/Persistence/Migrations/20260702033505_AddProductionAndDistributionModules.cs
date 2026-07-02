using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyaboo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionAndDistributionModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RemainingQuantity",
                table: "stock_assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: las asignaciones existentes conservan todo su saldo despachable
            migrationBuilder.Sql("UPDATE stock_assignments SET \"RemainingQuantity\" = \"Quantity\";");

            migrationBuilder.CreateTable(
                name: "confeccionistas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_confeccionistas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cutting_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cutting_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cutting_orders_product_references_ProductReferenceId",
                        column: x => x.ProductReferenceId,
                        principalTable: "product_references",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_orders_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cutting_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttingOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedQuantity = table.Column<int>(type: "integer", nullable: false),
                    CutQuantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cutting_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cutting_order_items_cutting_orders_CuttingOrderId",
                        column: x => x.CuttingOrderId,
                        principalTable: "cutting_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cutting_order_items_product_variants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sewing_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CuttingOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfeccionistaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sewing_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sewing_orders_confeccionistas_ConfeccionistaId",
                        column: x => x.ConfeccionistaId,
                        principalTable: "confeccionistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sewing_orders_cutting_orders_CuttingOrderId",
                        column: x => x.CuttingOrderId,
                        principalTable: "cutting_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_order_items_product_variants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_order_items_sales_orders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "sales_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sewing_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SewingOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantitySent = table.Column<int>(type: "integer", nullable: false),
                    QuantityApproved = table.Column<int>(type: "integer", nullable: false),
                    QuantityRejected = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sewing_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sewing_order_items_product_variants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sewing_order_items_sewing_orders_SewingOrderId",
                        column: x => x.SewingOrderId,
                        principalTable: "sewing_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_confeccionistas_Name",
                table: "confeccionistas",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_customers_Name",
                table: "customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_cutting_order_items_CuttingOrderId",
                table: "cutting_order_items",
                column: "CuttingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_cutting_order_items_ProductVariantId",
                table: "cutting_order_items",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_cutting_orders_OrderCode",
                table: "cutting_orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cutting_orders_ProductReferenceId",
                table: "cutting_orders",
                column: "ProductReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_order_items_ProductVariantId",
                table: "sales_order_items",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_order_items_SalesOrderId",
                table: "sales_order_items",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_CustomerId",
                table: "sales_orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_OrderCode",
                table: "sales_orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_Status",
                table: "sales_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_sewing_order_items_ProductVariantId",
                table: "sewing_order_items",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_sewing_order_items_SewingOrderId",
                table: "sewing_order_items",
                column: "SewingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_sewing_orders_ConfeccionistaId",
                table: "sewing_orders",
                column: "ConfeccionistaId");

            migrationBuilder.CreateIndex(
                name: "IX_sewing_orders_CuttingOrderId",
                table: "sewing_orders",
                column: "CuttingOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sewing_orders_OrderCode",
                table: "sewing_orders",
                column: "OrderCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cutting_order_items");

            migrationBuilder.DropTable(
                name: "sales_order_items");

            migrationBuilder.DropTable(
                name: "sewing_order_items");

            migrationBuilder.DropTable(
                name: "sales_orders");

            migrationBuilder.DropTable(
                name: "sewing_orders");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "confeccionistas");

            migrationBuilder.DropTable(
                name: "cutting_orders");

            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "stock_assignments");
        }
    }
}
