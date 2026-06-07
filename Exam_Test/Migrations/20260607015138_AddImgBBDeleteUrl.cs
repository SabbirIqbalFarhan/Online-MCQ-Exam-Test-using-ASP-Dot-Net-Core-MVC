using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam_Test.Migrations
{
    /// <inheritdoc />
    public partial class AddImgBBDeleteUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImgBBDeleteUrl",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgBBDeleteUrl",
                table: "Questions");
        }
    }
}
