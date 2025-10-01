using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LearnLangs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDictationOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictationTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictationTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDictationProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SetId = table.Column<int>(type: "int", nullable: false),
                    LastIndex = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDictationProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DictationSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictationSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictationSets_DictationTopics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "DictationTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DictationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetId = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Transcript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hint = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictationItems_DictationSets_SetId",
                        column: x => x.SetId,
                        principalTable: "DictationSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DictationTopics",
                columns: new[] { "Id", "CoverUrl", "Description", "Title" },
                values: new object[] { 2000, "/img/ielts.png", "Mini demo", "IELTS Listening (Demo)" });

            migrationBuilder.InsertData(
                table: "DictationSets",
                columns: new[] { "Id", "Level", "OrderIndex", "Title", "TopicId" },
                values: new object[] { 2100, "B2", 1, "Cam 20 – Test 1 – Part 1 (Demo)", 2000 });

            migrationBuilder.InsertData(
                table: "DictationItems",
                columns: new[] { "Id", "AudioUrl", "Hint", "OrderIndex", "SetId", "Transcript" },
                values: new object[,]
                {
                    { 2101, "/audio/demo1.mp3", null, 1, 2100, "I've been meaning to ask you for some advice about restaurants." },
                    { 2102, "/audio/demo2.mp3", null, 2, 2100, "I need to book somewhere to celebrate my sister's thirtieth birthday." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictationItems_SetId_OrderIndex",
                table: "DictationItems",
                columns: new[] { "SetId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_DictationSets_TopicId_OrderIndex",
                table: "DictationSets",
                columns: new[] { "TopicId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDictationProgresses_UserId_SetId",
                table: "UserDictationProgresses",
                columns: new[] { "UserId", "SetId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictationItems");

            migrationBuilder.DropTable(
                name: "UserDictationProgresses");

            migrationBuilder.DropTable(
                name: "DictationSets");

            migrationBuilder.DropTable(
                name: "DictationTopics");
        }
    }
}
