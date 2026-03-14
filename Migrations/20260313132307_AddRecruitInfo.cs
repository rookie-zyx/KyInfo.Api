using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyInfo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecruitInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PlanCount = table.Column<int>(type: "int", nullable: true),
                    ExamSubjects = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraRequirements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    MajorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitInfos_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecruitInfos_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecruitInfos_MajorId",
                table: "RecruitInfos",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitInfos_SchoolId",
                table: "RecruitInfos",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitInfos_Year_SchoolId_MajorId",
                table: "RecruitInfos",
                columns: new[] { "Year", "SchoolId", "MajorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecruitInfos");
        }
    }
}
