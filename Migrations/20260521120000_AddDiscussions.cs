using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyInfo.Api.Migrations;

/// <inheritdoc />
public partial class AddDiscussions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Discussions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                AuthorUserId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Discussions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Discussions_Users_AuthorUserId",
                    column: x => x.AuthorUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DiscussionComments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DiscussionId = table.Column<int>(type: "int", nullable: false),
                AuthorUserId = table.Column<int>(type: "int", nullable: false),
                Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscussionComments", x => x.Id);
                table.ForeignKey(
                    name: "FK_DiscussionComments_Discussions_DiscussionId",
                    column: x => x.DiscussionId,
                    principalTable: "Discussions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DiscussionComments_Users_AuthorUserId",
                    column: x => x.AuthorUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DiscussionComments_AuthorUserId",
            table: "DiscussionComments",
            column: "AuthorUserId");

        migrationBuilder.CreateIndex(
            name: "IX_DiscussionComments_DiscussionId_CreatedAt",
            table: "DiscussionComments",
            columns: new[] { "DiscussionId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Discussions_AuthorUserId",
            table: "Discussions",
            column: "AuthorUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Discussions_CreatedAt",
            table: "Discussions",
            column: "CreatedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DiscussionComments");
        migrationBuilder.DropTable(name: "Discussions");
    }
}
