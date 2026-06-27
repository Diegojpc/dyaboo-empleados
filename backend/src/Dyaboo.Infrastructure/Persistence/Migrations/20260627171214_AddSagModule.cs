using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dyaboo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSagModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OverheadPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_orders_product_references_ProductReferenceId",
                        column: x => x.ProductReferenceId,
                        principalTable: "product_references",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MaterialCostPerUnit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    LaborCostPerUnit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OverheadCostPerUnit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_order_items_product_variants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_order_items_production_orders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "production_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_production_order_items_ProductionOrderId",
                table: "production_order_items",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_items_ProductVariantId",
                table: "production_order_items",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_OrderCode",
                table: "production_orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_ProductReferenceId",
                table: "production_orders",
                column: "ProductReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "production_order_items");

            migrationBuilder.DropTable(
                name: "production_orders");
        }
    }
}
