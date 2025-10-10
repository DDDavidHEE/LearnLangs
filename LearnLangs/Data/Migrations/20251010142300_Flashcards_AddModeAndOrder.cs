using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LearnLangs.Data.Migrations
{
    /// <inheritdoc />
    public partial class Flashcards_AddModeAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "UserFlashcardProgresses");

            migrationBuilder.DeleteData(
                table: "FlashcardDecks",
                keyColumn: "Id",
                keyValue: 500);

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FlashcardDecks");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "FlashcardDecks");

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "FlashcardDecks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "FlashcardDecks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FlashcardCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeckId = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    FrontWord = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ipa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phonetic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackMeaningVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExampleEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExampleVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardCards_FlashcardDecks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "FlashcardDecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardDecks_Mode_OrderIndex",
                table: "FlashcardDecks",
                columns: new[] { "Mode", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardCards_DeckId_OrderIndex",
                table: "FlashcardCards",
                columns: new[] { "DeckId", "OrderIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardCards");

            migrationBuilder.DropIndex(
                name: "IX_FlashcardDecks_Mode_OrderIndex",
                table: "FlashcardDecks");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "FlashcardDecks");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "FlashcardDecks");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FlashcardDecks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "FlashcardDecks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeckId = table.Column<int>(type: "int", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Back = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Front = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_FlashcardDecks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "FlashcardDecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFlashcardProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Due = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EaseFactor = table.Column<double>(type: "float", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: false),
                    Repetition = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardProgresses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FlashcardDecks",
                columns: new[] { "Id", "CoverUrl", "Description", "IsPublic", "Title" },
                values: new object[] { 500, "/img/deck_basic.png", "Common words", true, "Basic English A1" });

            migrationBuilder.InsertData(
                table: "Flashcards",
                columns: new[] { "Id", "AudioUrl", "Back", "DeckId", "Front", "Hint", "ImageUrl", "OrderIndex" },
                values: new object[,]
                {
                    { 5001, null, "quả táo", 500, "apple", null, null, 1 },
                    { 5002, null, "cuốn sách", 500, "book", null, null, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_DeckId_OrderIndex",
                table: "Flashcards",
                columns: new[] { "DeckId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId_CardId",
                table: "UserFlashcardProgresses",
                columns: new[] { "UserId", "CardId" },
                unique: true);
        }
    }
}
