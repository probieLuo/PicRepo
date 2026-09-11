using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PicRepo.Client.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHisTable0907 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "UploadHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommitMessage",
                table: "UploadHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Repo",
                table: "UploadHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "UploadHistory");

            migrationBuilder.DropColumn(
                name: "CommitMessage",
                table: "UploadHistory");

            migrationBuilder.DropColumn(
                name: "Repo",
                table: "UploadHistory");
        }
    }
}