using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSaysay.Migrations
{
    /// <inheritdoc />
    public partial class AddedExerciseOrdertoUserProg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExerciseOrder",
                table: "UserProgress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseOrder",
                table: "UserProgress");
        }
    }
}
