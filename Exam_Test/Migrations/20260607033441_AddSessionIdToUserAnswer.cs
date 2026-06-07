using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam_Test.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToUserAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "UserAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "UserAnswers");
        }
    }
}
