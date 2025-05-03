using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanabel.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "security",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "security",
                table: "Users");
        }
    }
}
