using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam_Test.Migrations
{
    /// <inheritdoc />
    public partial class AddImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageData",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Questions");
        }
    }
}
