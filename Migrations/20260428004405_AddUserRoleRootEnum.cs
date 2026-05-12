using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyInfo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleRootEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 将默认种子管理员（用户名 admin、角色仍为 Admin=1）提升为 Root=2；
            // 若库中已有其他 Root，则跳过以免产生多个 Root。
            migrationBuilder.Sql(
                """
                UPDATE u
                SET u.Role = 2
                FROM Users AS u
                WHERE u.Role = 1
                  AND u.UserName = N'admin'
                  AND NOT EXISTS (SELECT 1 FROM Users AS r WHERE r.Role = 2 AND r.Id <> u.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Users
                SET Role = 1
                WHERE Role = 2 AND UserName = N'admin';
                """);
        }
    }
}
