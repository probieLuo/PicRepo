using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PicRepo.Client.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHisTable0916 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UploadHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UploadHistory");
        }
    }
}
