using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DurableFunctionExample.Migrations
{
    /// <inheritdoc />
    public partial class totalPriceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                schema: "Tienda",
                table: "Order",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                schema: "Tienda",
                table: "Order");
        }
    }
}
