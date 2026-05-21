using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyInfo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RatingSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    AvgScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalRatings = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RatingScoreDistributions",
                columns: table => new
                {
                    RatingSubjectId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<byte>(type: "tinyint", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingScoreDistributions", x => new { x.RatingSubjectId, x.Score });
                    table.ForeignKey(
                        name: "FK_RatingScoreDistributions_RatingSubjects_RatingSubjectId",
                        column: x => x.RatingSubjectId,
                        principalTable: "RatingSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RatingSubjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRatings_RatingSubjects_RatingSubjectId",
                        column: x => x.RatingSubjectId,
                        principalTable: "RatingSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RatingSubjects_SubjectType_SubjectId",
                table: "RatingSubjects",
                columns: new[] { "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_RatingSubjectId_UserId",
                table: "UserRatings",
                columns: new[] { "RatingSubjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_UserId",
                table: "UserRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RatingScoreDistributions");

            migrationBuilder.DropTable(
                name: "UserRatings");

            migrationBuilder.DropTable(
                name: "RatingSubjects");
        }
    }
}
