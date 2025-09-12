using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnLangs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakRewardFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Has30DayStreakReward",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Has7DayStreakReward",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Has30DayStreakReward",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Has7DayStreakReward",
                table: "AspNetUsers");
        }
    }
}
