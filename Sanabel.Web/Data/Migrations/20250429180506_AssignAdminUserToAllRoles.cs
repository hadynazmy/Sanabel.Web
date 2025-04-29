using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanabel.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssignAdminUserToAllRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into [security].[UserRoles] (UserId , RoleId) Select '73afbcbc-692a-4a9d-8337-d27a17ad68ad', Id From [security].[roles] ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete From  [security].[UserRoles] where UserId = '73afbcbc-692a-4a9d-8337-d27a17ad68ad'");
        }
    }
}
