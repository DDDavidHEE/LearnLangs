using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LearnLangs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChineseCourseSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLessons_UserId",
                table: "UserLessons");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Lessons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 100, "Beginner Mandarin: greetings, numbers, self-intro with pinyin.", "Chinese course" });

            migrationBuilder.InsertData(
                table: "Lessons",
                columns: new[] { "Id", "CourseId", "OrderIndex", "Title", "XpReward" },
                values: new object[,]
                {
                    { 101, 100, 1, "Lesson 1 – Greetings", 30 },
                    { 102, 100, 2, "Lesson 2 – Numbers 1–10", 30 },
                    { 103, 100, 3, "Lesson 3 – Self-Introduction", 40 }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CorrectAnswer", "FillInTheBlankAnswer", "IsMultipleChoice", "LessonId", "OptionA", "OptionB", "OptionC", "OptionD", "Prompt", "ShortAnswer" },
                values: new object[,]
                {
                    { 1001, "B", null, true, 101, "Tạm biệt", "Xin chào", "Cảm ơn", "Xin lỗi", "“你好” nghĩa là gì?", null },
                    { 1002, "A", null, true, 101, "Chào buổi sáng", "Chúc ngủ ngon", "Chúc mừng", "Hẹn gặp lại", "“早上好” nghĩa là…", null },
                    { 1003, "B", null, true, 101, "Bạn tên gì?", "Bạn khỏe không?", "Bạn ở đâu?", "Bạn bao nhiêu tuổi?", "“你好吗？” nghĩa là…", null },
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

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_UserId_LessonId",
                table: "UserLessons",
                columns: new[] { "UserId", "LessonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLessons_UserId_LessonId",
                table: "UserLessons");

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1003);

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

            migrationBuilder.DeleteData(
                table: "Lessons",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Lessons",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Lessons",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Lessons",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_UserId",
                table: "UserLessons",
                column: "UserId");
        }
    }
}
