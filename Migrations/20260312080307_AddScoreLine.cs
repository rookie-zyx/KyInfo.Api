using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyInfo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoreLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsNational = table.Column<bool>(type: "bit", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    MajorId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreLines_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScoreLines_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreLines_MajorId",
                table: "ScoreLines",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreLines_SchoolId",
                table: "ScoreLines",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreLines_Year_IsNational",
                table: "ScoreLines",
                columns: new[] { "Year", "IsNational" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoreLines");
        }
    }
}
