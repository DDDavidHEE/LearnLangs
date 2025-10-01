using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnLangs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettingsToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultPronunciationLang",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultTranslateFrom",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultTranslateTo",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredUiTheme",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPronunciationRawJson",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPronunciationLang",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultTranslateFrom",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultTranslateTo",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PreferredUiTheme",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ShowPronunciationRawJson",
                table: "AspNetUsers");
        }
    }
}
