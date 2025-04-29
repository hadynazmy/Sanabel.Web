using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanabel.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO [security].[Users] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed],
                [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FirstName], [LastName],
                [ProfilePicture]) VALUES (N'73afbcbc-692a-4a9d-8337-d27a17ad68ad', N'admin44', N'ADMIN44', N'admin44@admin.com',
                N'ADMIN44@ADMIN.COM', 0, N'AQAAAAIAAYagAAAAEPouf4StyqDCpmj8JPNTxQDwdZX5SphuI+7g2hwAqqRsju1HQ8sQCpfZFzRTaE6GbA==',
                N'HORPZRX4IXD66XQMLDVODXDFDBYV7WLB', N'9024f37b-058f-4401-990d-b24064c5c278', NULL, 0, 0, NULL, 1, 0, N'hady',
                N'nazmy', 0x00)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [security].[Users] WHERE Id= '73afbcbc-692a-4a9d-8337-d27a17ad68ad'");
        }
    }
}
