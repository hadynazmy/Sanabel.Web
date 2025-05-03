using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanabel.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCoulmPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "PaymentImage",
                table: "Orders",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "PaymentImageName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentImageType",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCost",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentImage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentImageName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentImageType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "Orders");
        }
    }
}
