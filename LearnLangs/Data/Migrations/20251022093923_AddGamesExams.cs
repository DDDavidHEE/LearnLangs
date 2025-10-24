using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LearnLangs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGamesExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1004);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1005);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1011);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1012);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1013);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1014);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1015);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1021);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1022);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1023);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1024);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1025);

            migrationBuilder.CreateTable(
                name: "GameLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GameLevelId = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    Correct = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    GameLevelId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_GameLevels_GameLevelId",
                        column: x => x.GameLevelId,
                        principalTable: "GameLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameLevelId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CorrectText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SentenceShuffled = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SentenceAnswer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerMapJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameQuestions_GameLevels_GameLevelId",
                        column: x => x.GameLevelId,
                        principalTable: "GameLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAttempts_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_ExamId",
                table: "ExamAttempts",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_UserId_ExamId",
                table: "ExamAttempts",
                columns: new[] { "UserId", "ExamId" });

            migrationBuilder.CreateIndex(
                name: "IX_Exams_GameLevelId",
                table: "Exams",
                column: "GameLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameQuestions_GameLevelId",
                table: "GameQuestions",
                column: "GameLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_UserId_GameLevelId",
                table: "GameResults",
                columns: new[] { "UserId", "GameLevelId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamAttempts");

            migrationBuilder.DropTable(
                name: "GameQuestions");

            migrationBuilder.DropTable(
                name: "GameResults");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "GameLevels");

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CorrectAnswer", "FillInTheBlankAnswer", "IsMultipleChoice", "LessonId", "OptionA", "OptionB", "OptionC", "OptionD", "Prompt", "ShortAnswer" },
                values: new object[,]
                {
                    { 1004, "A", null, true, 101, "我很好，谢谢。", "对不起。", "再见。", "没关系。", "Trả lời lịch sự cho “你好吗？”", null },
                    { 1005, "B", null, true, 101, "Cảm ơn", "Tạm biệt", "Xin chào", "Không sao", "“再见” nghĩa là…", null },
                    { 1011, "B", null, true, 102, "3", "4", "5", "6", "Số “四” là số nào?", null },
                    { 1012, "A", null, true, 102, "bā", "bá", "bǎ", "bà", "Pinyin đúng của “八” là…", null },
                    { 1013, "D", null, true, 102, "6", "7", "8", "9", "“九” là…", null },
                    { 1014, "A", null, true, 102, "六", "九", "二", "十", "“liù” viết Hán tự là…", null },
                    { 1015, "C", null, true, 102, "百", "千", "十", "万", "Số “10” trong tiếng Trung là…", null },
                    { 1021, "C", null, true, 103, "Nói tuổi", "Nói quê quán", "Nói tên", "Nói nghề nghiệp", "“我叫…” dùng khi nào?", null },
                    { 1022, "A", null, true, 103, "你叫什么名字？", "你多大？", "你来自哪里？", "你做什么工作？", "Câu hỏi để hỏi tên người khác:", null },
                    { 1023, "A", null, true, 103, "Tôi đến từ Việt Nam", "Tôi yêu Việt Nam", "Tôi ở Việt Nam", "Tôi nói tiếng Việt", "“我来自越南。” nghĩa là…", null },
                    { 1024, "B", null, true, 103, "I am a teacher", "I am a student", "I am from China", "My name is…", "“我是学生。” tương đương…", null },
                    { 1025, "C", null, true, 103, "Cảm ơn", "Xin lỗi", "Rất vui được gặp bạn", "Hẹn gặp lại", "“很高兴认识你。” nghĩa là…", null }
                });
        }
    }
}
